using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.WebSocket;
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

        public MeetingController(
            IMeetingInfoService meetingInfoService, 
            ChannelContextUtils channelContextUtils)
        {
            _meetingInfoService = meetingInfoService;
            _channelContextUtils = channelContextUtils;
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
            var data = await _meetingInfoService.QuickMeetingAsync(meetingInput);

            return new ResponseDto(data);
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
            _channelContextUtils.SendMessage(data);

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
            var data = await _meetingInfoService.PreJoinMeetingAsync(meetingInput);

            return new ResponseDto(data);
        }
    }
}
