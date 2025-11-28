using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Concurrency;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Enums;
using MeetLive.Services.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MeetLive.Services.WebSocket
{
    public class ChannelContextUtils
    {
        //用户
        public static ConcurrentDictionary<string, IChannel> USER_CONTEXT_MAP = new ConcurrentDictionary<string, IChannel>();

        //会议
        public static ConcurrentDictionary<string, IChannelGroup> MEETING_ROOM_CONTEXT_MAP = new ConcurrentDictionary<string, IChannelGroup>();

        private readonly ILogger<ChannelContextUtils> _logger;
        private readonly IUserInfoRepository _userInfoRepository;
        private static readonly IEventExecutor GlobalEventExecutor = new SingleThreadEventLoop();

        public ChannelContextUtils(
            ILogger<ChannelContextUtils> logger,
            IUserInfoRepository userInfoRepository)
        {
            _logger = logger;
            _userInfoRepository = userInfoRepository;
        }

        public async void AddContext(string userId, IChannel channel)
        {
            try
            {
                string channelId = channel!.Id!.ToString()!;

                // 创建或获取 AttributeKey
                AttributeKey<string> attributeKey = AttributeKey<string>.ValueOf(channelId);

                // 设置用户ID到channel属性
                channel.GetAttribute(attributeKey).Set(userId);

                // 添加到用户-通道映射
                USER_CONTEXT_MAP.AddOrUpdate(userId, channel, (key, oldValue) => channel);

                //更新用户最后登录时间
                await _userInfoRepository.UpdateLastLoginTimeAsync(Convert.ToInt64(userId), DateTime.Now);

                var userDto = CacheManager.Get<UserInfoDto>(RedisKeyPrefix.Redis_Key_Ws_Token_UserId + userId);
                if (userDto == null || userDto.CurrentMeetingId == null)
                {
                    return;
                }

                //自动加入会议
                AddJoinMeetingRoom(userDto.CurrentMeetingId, userId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 加入会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="userId"></param>
        public void AddJoinMeetingRoom(string meetingId, string userId)
        {
            // 从用户映射中获取对应的 Channel
            if (!USER_CONTEXT_MAP.TryGetValue(userId, out IChannel? userChannel) || userChannel == null)
            {
                _logger.LogWarning("用户 {UserId} 的 Channel 不存在，无法加入会议 {MeetingId}", userId, meetingId);
                return;
            }

            // 获取或创建会议室的 ChannelGroup
            IChannelGroup group = MEETING_ROOM_CONTEXT_MAP.GetOrAdd(meetingId, key =>
            {
                _logger.LogInformation("创建新的会议 ChannelGroup: {MeetingId}", meetingId);
                return new DefaultChannelGroup(GlobalEventExecutor);
            });

            IChannel channel = group.Find(userChannel.Id);
            if (channel == null)
            {
                group.Add(channel);
            }
        }

        /// <summary>
        /// 发消息
        /// </summary>
        /// <param name="messageSendDto">消息内容</param>
        public void SendMessage(MessageSendDto<object> messageSendDto)
        {
            if (messageSendDto.MessageSendType == MessageSendTypeEnum.USER)
            {
                SendMessageToUser(messageSendDto);
            }
            else
            {
                SendMessageToGroup(messageSendDto);
            }
        }

        /// <summary>
        /// 给某个组里面发送消息
        /// </summary>
        public void SendMessageToGroup(MessageSendDto<object> messageSendDto)
        {
            if (string.IsNullOrWhiteSpace(messageSendDto.MeetingId))
            {
                return;
            }

            var exists = MEETING_ROOM_CONTEXT_MAP.TryGetValue(messageSendDto.MeetingId, out IChannelGroup? channelGroup);
            if (exists && channelGroup != null)
            {
                TextWebSocketFrame messageText = new TextWebSocketFrame(JsonConvert.SerializeObject(messageSendDto));
                channelGroup.WriteAndFlushAsync(messageText);
            }

            //退出会议
            if (messageSendDto.MessageType == MessageTypeEnum.EXIT_MEETING_ROOM)
            {
                var exitDto = messageSendDto.MessageContent as MeetingExitDto;
                if (exitDto != null)
                {
                    RemoveContextFromGroup(exitDto.ExitUserId.ToString(), messageSendDto.MeetingId);

                    var allMember = RedisComponent.GetMeetingMemberList(messageSendDto.MeetingId);
                    int onLiveCount = allMember.Where(t => t.Status == 1).Count();
                    if (onLiveCount == 0)
                    {
                        RemoveContextGroup(messageSendDto.MeetingId);
                    }
                }
                return;
            }
            //结束会议
            if (messageSendDto.MessageType == MessageTypeEnum.FINIS_MEETING)
            {
                var allMember = RedisComponent.GetMeetingMemberList(messageSendDto.MeetingId);
                foreach (var item in allMember)
                {
                    RemoveContextFromGroup(item.UserId.ToString(), messageSendDto.MeetingId);
                }
                RemoveContextGroup(messageSendDto.MeetingId);
            }
        }

        /// <summary>
        /// 给某个用户发消息
        /// </summary>
        public void SendMessageToUser(MessageSendDto<object> messageSendDto)
        {
            if (string.IsNullOrWhiteSpace(messageSendDto.ReceiveUserId))
            {
                return;
            }

            // 从用户映射中获取对应的 Channel
            if (!USER_CONTEXT_MAP.TryGetValue(messageSendDto.ReceiveUserId, out IChannel? userChannel) || userChannel == null)
            {
                return;
            }

            TextWebSocketFrame messageText = new TextWebSocketFrame(JsonConvert.SerializeObject(messageSendDto));
            userChannel.WriteAndFlushAsync(messageText);
        }


        /// <summary>
        /// 断开链接,关闭链接
        /// </summary>
        /// <param name="userId"></param>
        public void CloseContext(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }
            // 从用户映射中获取对应的 Channel
            if (!USER_CONTEXT_MAP.TryGetValue(userId, out IChannel? userChannel) || userChannel == null)
            {
                return;
            }

            USER_CONTEXT_MAP.Remove(userId, out userChannel);
            if (userChannel != null)
            {
                userChannel.CloseAsync();
            }
        }

        /// <summary>
        /// 移除群组里的人
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="meetingId"></param>
        private void RemoveContextFromGroup(string userId, string meetingId)
        {
            if (!USER_CONTEXT_MAP.ContainsKey(userId)) return;

            var context = USER_CONTEXT_MAP[userId];
            if (context == null)
            {
                return;
            }

            if (!MEETING_ROOM_CONTEXT_MAP.ContainsKey(meetingId)) return;

            var group = MEETING_ROOM_CONTEXT_MAP[meetingId];
            if (group != null)
            {
                group.Remove(context);
            }
        }

        /// <summary>
        /// 移除群组
        /// </summary>
        /// <param name="meetingId"></param>
        private void RemoveContextGroup(string meetingId)
        {
            if (!MEETING_ROOM_CONTEXT_MAP.ContainsKey(meetingId)) return;

            MEETING_ROOM_CONTEXT_MAP.Remove(meetingId, out _);
        }
    }
}
