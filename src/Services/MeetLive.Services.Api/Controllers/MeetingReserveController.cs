using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    /// <summary>
    /// 预约会议
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MeetingReserveController : ControllerBase
    {
        private readonly IMeetingReserveService _meetingReserveService;

        public MeetingReserveController(IMeetingReserveService meetingReserveService)
        {
            _meetingReserveService = meetingReserveService;
        }

        /// <summary>
        /// 加载我的预约会议
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadMyMeetingReserve()
        {
            var data = await _meetingReserveService.LoadMyMeetingReserveAsync();

            return new ResponseDto(data);
        }

        /// <summary>
        /// 创建预约会议
        /// </summary>
        /// <param name="meetingReserveInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> CreateMeetingReserve(MeetingReserveInput meetingReserveInput)
        {
            await _meetingReserveService.CreateMeetingReserveAsync(meetingReserveInput);

            return new ResponseDto();
        }

        /// <summary>
        /// 删除预约会议(创建人删除)
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> DeleteMeetingReserve(string meetingId)
        {
            await _meetingReserveService.DeleteMeetingReserveAsync(meetingId);

            return new ResponseDto();
        }

        /// <summary>
        /// 参加人删除预约会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> DeleteMeetingReserveByUser(string meetingId)
        {
            await _meetingReserveService.DeleteMeetingReserveByUserAsync(meetingId);

            return new ResponseDto();
        }

        /// <summary>
        /// 加载今天要参加的会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadTodayMeeting()
        {
            await _meetingReserveService.LoadTodayMeetingAsync();

            return new ResponseDto();
        }
    }
}
