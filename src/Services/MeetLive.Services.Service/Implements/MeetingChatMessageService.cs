using MeetLive.Services.Common;
using MeetLive.Services.Common.Captcha;
using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Enums;
using MeetLive.Services.IService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetLive.Services.Service.Implements
{
    public class MeetingChatMessageService : ServiceBase, IMeetingChatMessageService
    {
        private readonly IDapperRepository _dapperRepository;
        private readonly IMeetingMemberRepository _memberRepository;
        private readonly FFmpegUtils _ffmpegUtils;

        public MeetingChatMessageService(
            IDapperRepository dapperRepository,
            FFmpegUtils ffmpegUtils,
            IMeetingMemberRepository memberRepository)
        {
            _dapperRepository = dapperRepository;
            _ffmpegUtils = ffmpegUtils;
            _memberRepository = memberRepository;
        }

        /// <summary>
        /// 加载聊天消息
        /// </summary>
        /// <param name="chatMessageQuery"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<List<MeetingChatMessageDto>> LoadChatMessagesAsync(ChatMessageQuery chatMessageQuery)
        {
            if (CurrentMeetingId == null)
            {
                throw new BusinessException("无会议id");
            }
            string tableName = TableSplitUtils.GetMeetingChatMessageTable(CurrentMeetingId);

            string sql = $@"select * from {tableName} 
                            where MessageId <= {chatMessageQuery.MaxMessageId} 
                            and (ReceiveUserId = {LoginUserId} or ReceiveType = 0)
                            order by MessageId desc
                            limit {chatMessageQuery.PageSize} 
                            offset {(chatMessageQuery.PageIndex - 1) * chatMessageQuery.PageSize}";

            var chatMessage = await _dapperRepository.QueryAsync<MeetingChatMessageDto>(sql);

            return chatMessage.ToList();
        }

        /// <summary>
        /// 加载历史消息
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public async Task<List<MeetingChatMessageDto>> LoadHistoryMessagesAsync(ChatMessageQuery chatMessageQuery)
        {
            if (chatMessageQuery.MeetingId == null)
            {
                throw new BusinessException("参数错误");
            }
            //判断这个人在不在这个会议里面
            int count = await _memberRepository.QueryWhere(t => t.MeetingId == chatMessageQuery.MeetingId.Value && t.UserId == LoginUserId).CountAsync();
            if (count == 0)
            {
                throw new BusinessException("参数错误");
            }

            string tableName = TableSplitUtils.GetMeetingChatMessageTable(chatMessageQuery.MeetingId.Value.ToString());

            string sql = $@"select * from {tableName} 
                            where MeetingId == {chatMessageQuery.MeetingId.Value} 
                            and (ReceiveUserId = {LoginUserId} or ReceiveType = 0)
                            order by MessageId desc
                            limit {chatMessageQuery.PageSize} 
                            offset {(chatMessageQuery.PageIndex - 1) * chatMessageQuery.PageSize}";

            var chatMessage = await _dapperRepository.QueryAsync<MeetingChatMessageDto>(sql);

            return chatMessage.ToList();
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sendMessageInput"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<(MeetingChatMessage, List<MessageSendDto<object>>)> SendMessageAsync(SendMessageInput sendMessageInput)
        {
            MeetingChatMessage chatMessage = new MeetingChatMessage
            {
                MessageId = SnowIdWorker.NextId(),
                MeetingId = Convert.ToInt64(CurrentMeetingId),
                MessageType = sendMessageInput.MessageType,
                MessageContent = sendMessageInput.Message,
                SendUserId = LoginUserId,
                SendUserName = LoginUserName,
                ReceiveUserId = sendMessageInput.ReceiveUserId,
                ReceiveType = sendMessageInput.ReceiveUserId == 0 ? 0 : 1,
                FileSize = sendMessageInput.FileSize,
                FileName = sendMessageInput.FileName,
                FileType = sendMessageInput.FileType
            };

            if (CurrentMeetingId == null)
            {
                throw new BusinessException("无会议id");
            }

            //消息类型只能是文本消息或媒体消息
            if (new List<int> { (int)MessageTypeEnum.CHAT_TEXT_MESSAGE, (int)MessageTypeEnum.CHAT_MEDIA_MESSAGE }.Contains(chatMessage.MessageType))
            {
                throw new BusinessException("参数错误");
            }

            //接收类型
            if (chatMessage.ReceiveType > 1 || chatMessage.ReceiveType < 0)
            {
                throw new BusinessException("参数错误");
            }

            MessageTypeEnum messageTypeEnum = (MessageTypeEnum)chatMessage.MessageType;
            //文本消息
            if (messageTypeEnum == MessageTypeEnum.CHAT_TEXT_MESSAGE)
            {
                if (string.IsNullOrWhiteSpace(chatMessage.MessageContent))
                {
                    throw new BusinessException("参数错误");
                }
                chatMessage.Status = 1;
            }
            //媒体消息(文件消息)
            else if (messageTypeEnum == MessageTypeEnum.CHAT_MEDIA_MESSAGE)
            {
                if (string.IsNullOrWhiteSpace(chatMessage.FileName) || chatMessage.FileSize == null || chatMessage.FileType == null)
                {
                    throw new BusinessException("参数错误");
                }
                chatMessage.Status = 0;
                chatMessage.FileSuffix = Path.GetExtension(chatMessage.FileName);
            }
            chatMessage.SendTime = DateTime.Now;

            string tableName = TableSplitUtils.GetMeetingChatMessageTable(CurrentMeetingId);
            string sql = $@"
                        INSERT INTO {tableName} 
                        (MessageId, MeetingId, MessageType, MessageContent, SendUserId, SendUserName, SendTime,
                         ReceiveUserId, ReceiveType, FileSize, FileName, FileType, FileSuffix, Status)
                        VALUES 
                        (@MessageId, @MeetingId, @MessageType, @MessageContent, @SendUserId, @SendUserName,@SendTime,
                         @ReceiveUserId, @ReceiveType, @FileSize, @FileName, @FileType, @FileSuffix, @Status)";
            int affectedRows = await _dapperRepository.ExecuteAsync(sql, chatMessage);

            List<MessageSendDto<object>> messageSendDtos = new List<MessageSendDto<object>>();
            //给指定的人发
            if (chatMessage.ReceiveType == 1)
            {
                var sendToUser = ObjectMapper.Map<MessageSendDto<object>>(chatMessage);
                sendToUser.MessageSendType = MessageSendTypeEnum.USER;
                messageSendDtos.Add(sendToUser); //给指定的人发

                var sendToMySelf = ObjectMapper.Map<MessageSendDto<object>>(chatMessage);
                sendToUser.MessageSendType = MessageSendTypeEnum.USER;
                sendToUser.ReceiveUserId = LoginUserId.ToString();
                messageSendDtos.Add(sendToUser); //给自己发
            }
            //给全员发
            else
            {
                var send = ObjectMapper.Map<MessageSendDto<object>>(chatMessage);
                send.MessageSendType = MessageSendTypeEnum.GROUP;
                messageSendDtos.Add(send);
            }

            return (chatMessage, messageSendDtos);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="uploadFileInput"></param>
        /// <returns></returns>
        public async Task<MessageSendDto<object>> UploadFileAsync(UploadFileInput uploadFileInput)
        {
            //文件路径/年月/会议id=>文件夹
            string month = DateTime.Now.ToString("yyyyMM");
            string folder = Path.Combine(FolderPath.PhysicalPath, month, CurrentMeetingId!);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //上传的文件名
            string fileName = uploadFileInput.File.FileName;
            //文件后缀
            string fileSuffix = Path.GetExtension(fileName);

            //文件重命名
            string resetName = uploadFileInput.MessageId.ToString();

            FileTypeEnum? fileTypeEnum = null;
            switch (fileSuffix)
            {
                case ".jpeg":
                case ".jpg":
                case ".png":
                case ".gif":
                case ".bmp":
                case ".webp":
                    fileTypeEnum = FileTypeEnum.Image;
                    break;
                case ".mp4":
                case ".avi":
                case ".rmvb":
                case ".mkv":
                case ".mov":
                    fileTypeEnum = FileTypeEnum.Video;
                    break;
            }

            // 创建临时文件
            string tempFileName = $"{resetName}_temp{fileSuffix}";
            string tempFilePath = Path.Combine(folder, tempFileName);
            // 保存上传的临时文件
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await uploadFileInput.File.CopyToAsync(stream);
            }

            var tempFileInfo = new FileInfo(tempFilePath);

            //最后保存到磁盘上的文件
            string finalFilePath = "";

            //图片
            if (fileTypeEnum == FileTypeEnum.Image)
            {
                // 转换为标准格式（如PNG转JPG）
                string targetExtension = ".jpg";
                string targetFileName = $"{resetName}{targetExtension}";
                finalFilePath = Path.Combine(folder, targetFileName);

                finalFilePath = _ffmpegUtils.TransferImageType(tempFileInfo, finalFilePath);
                // 生成缩略图=>$"{resetName}_thumb{targetExtension}"
                _ffmpegUtils.CreateImageThumbnail(finalFilePath);
            }
            //视频
            else if (fileTypeEnum == FileTypeEnum.Video)
            {
                // 转换为MP4格式
                string targetExtension = ".mp4";
                string targetFileName = $"{resetName}{targetExtension}";
                finalFilePath = Path.Combine(folder, targetFileName);

                // 调用ffmpegUtils转换视频格式
                await _ffmpegUtils.TransferVideoTypeAsync(tempFileInfo, finalFilePath, fileSuffix);

                // 生成视频缩略图
                await _ffmpegUtils.CreateVideoThumbnailAsync(finalFilePath);
            }
            //其它
            else
            {
                string targetFileName = $"{resetName}{fileSuffix}";
                finalFilePath = Path.Combine(folder, targetFileName);
                // 直接保存文件
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    await uploadFileInput.File.CopyToAsync(stream);
                }
            }

            //删除临时文件
            if (tempFileInfo.Exists)
            {
                tempFileInfo.Delete();
            }

            //修改消息表里的数据
            string tableName = TableSplitUtils.GetMeetingChatMessageTable(CurrentMeetingId!);
            string sql = "update @tableName set Status=1 where MessageId=@MeeageId";
            int affectedRows = await _dapperRepository.ExecuteAsync(sql, new { tableName = tableName, MeeageId = uploadFileInput.MessageId });

            MessageSendDto<object> messageSendDto = new MessageSendDto<object>
            {
                MeetingId = CurrentMeetingId,
                MessageType = MessageTypeEnum.CHAT_MEDIA_MEAASGE_UPDATE,
                Status = 1,
                MessageId = uploadFileInput.MessageId,
                MessageSendType = MessageSendTypeEnum.GROUP,
            };

            return messageSendDto;
        }
    }
}
