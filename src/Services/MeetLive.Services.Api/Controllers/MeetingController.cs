using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.Service;
using MeetLive.Services.WebSocket;
using MeetLive.Services.WebSocket.Message;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class MeetingController : BaseController
    {
        private readonly IMeetingInfoService _meetingInfoService;
        private readonly ChannelContextUtils _channelContextUtils;
        private readonly IMessageHandler _messageHandler;

        public MeetingController(
            IMeetingInfoService meetingInfoService,
            ChannelContextUtils channelContextUtils,
            IMessageHandler messageHandler)
        {
            _meetingInfoService = meetingInfoService;
            _channelContextUtils = channelContextUtils;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// 获取参与的会议
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadMeeting(MeetingQueryInput queryInput)
        {
            var data = await _meetingInfoService.LoadMeetingAsync(queryInput);

            return new ResponseDto(data);
        }

        /// <summary>
        /// 创建会议
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> QuickMeeting(QuickMeetingInput meetingInput)
        {
            await _meetingInfoService.QuickMeetingAsync(meetingInput);

            return new ResponseDto();
        }

        /// <summary>
        /// 加入会议(发起人加入会议)
        /// </summary>
        /// <param name="videoOpen">是否打开摄像头</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> JoinMeeting(bool videoOpen)
        {
            var data = await _meetingInfoService.JoinMeetingAsync(videoOpen);

            //加入ws房间
            _channelContextUtils.AddJoinMeetingRoom(data.MeetingId!, LoginUserId.ToString());

            //发送ws消息
            _messageHandler.SendMessage(data);
            //_channelContextUtils.SendMessage(data);

            return new ResponseDto();
        }

        /// <summary>
        /// 预加入会议(其他人加入会议)
        /// </summary>
        /// <param name="videoOpen">是否打开摄像头</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> PreJoinMeeting(PreJoinMeetingInput meetingInput)
        {
            await _meetingInfoService.PreJoinMeetingAsync(meetingInput);

            return new ResponseDto();
        }

        /// <summary>
        /// 退出会议
        /// </summary>
        /// <param name="type">2-退出会议,3-被踢出会议,4-被拉黑</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> ExitMeeting()
        {
            var userDto = RedisComponent.GetUserInfoByUserId(LoginUserId.ToString());
            if (userDto == null)
            {
                return new ResponseDto(false, "参数错误");
            }
            var data = await _meetingInfoService.ExitMeetingAsync(userDto, 2);

            if (data != null)
            {
                //发送消息
                _messageHandler.SendMessage(data);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 踢出会议
        /// </summary>
        /// <param name="type">2-退出会议,3-被踢出会议,4-被拉黑</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> KickOutMeeting(string userId)
        {
            var data = await _meetingInfoService.ForceExitMeetingAsync(3, userId);

            if (data != null)
            {
                //发送消息
                _messageHandler.SendMessage(data);
            }

            return new ResponseDto(data);
        }

        /// <summary>
        /// 被拉黑
        /// </summary>
        /// <param name="type">2-退出会议,3-被踢出会议,4-被拉黑</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> BlackMeeting(string userId)
        {
            var data = await _meetingInfoService.ForceExitMeetingAsync(4, userId);

            if (data != null)
            {
                //发送消息
                _messageHandler.SendMessage(data);
            }

            return new ResponseDto(data);
        }

        /// <summary>
        /// 获取当前正在进行的会议
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> GetCurrentMeeting()
        {
            var userDto = RedisComponent.GetUserInfoByUserId(LoginUserId.ToString());
            if (userDto == null || string.IsNullOrWhiteSpace(userDto.CurrentMeetingId))
            {
                return new ResponseDto();
            }

            var meeting = await _meetingInfoService.GetCurrentMeetingAsync();
            if (meeting == null || meeting.Status == 0)
            {
                return new ResponseDto();
            }

            return new ResponseDto(meeting);
        }

        /// <summary>
        /// 结束会议
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> FinishMeeting()
        {
            var userDto = RedisComponent.GetUserInfoByUserId(LoginUserId.ToString());
            if (userDto == null || string.IsNullOrWhiteSpace(userDto.CurrentMeetingId))
            {
                return new ResponseDto();
            }

            var data = await _meetingInfoService.FinishMeetingAsync(userDto.CurrentMeetingId);

            //发送消息
            _messageHandler.SendMessage(data);

            return new ResponseDto();
        }

        /// <summary>
        /// 删除会议记录
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> DeleteMeetingRecord(string meetingId)
        {
            await _meetingInfoService.DeleteMeetingRecordAsync(meetingId);

            return new ResponseDto();
        }

        /// <summary>
        /// 加载会议成员
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadMeetingMember(string meetingId)
        {
            var data = await _meetingInfoService.LoadMeetingMemberAsync(meetingId);
            if (data.Any(t => t.UserId == LoginUserId))
            {
                return new ResponseDto(data);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 参加预约会议
        /// </summary>
        /// <param name="reserveJoinMeetingInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> ReserveJoinMeeting(ReserveJoinMeetingInput reserveJoinMeetingInput)
        {
            await _meetingInfoService.ReserveJoinMeetingAsync(reserveJoinMeetingInput);

            return new ResponseDto();
        }

        /// <summary>
        /// 邀请入会
        /// </summary>
        /// <param name="selectContactIds"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> InviteMember(string selectContactIds)
        {
            var data = await _meetingInfoService.InviteMemberAsync(selectContactIds);

            foreach (var item in data)
            {
                _messageHandler.SendMessage(item);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 接受邀请
        /// </summary>
        /// <param name="selectContactIds"></param>
        /// <returns></returns>
        [HttpPost]
        public ResponseDto AcceptInvite(long meetingId)
        {
            _meetingInfoService.AcceptInviteAsync(meetingId);

            return new ResponseDto();
        }
    }
}
