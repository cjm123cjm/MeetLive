using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.Service;
using MeetLive.Services.WebSocket.Message;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    /// <summary>
    /// 管理员接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = "IsAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IMessageHandler _messageHandler;

        public AdminController(IAdminService adminService, IMessageHandler messageHandler)
        {
            _adminService = adminService;
            _messageHandler = messageHandler;
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="queryInput"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadUserList([FromQuery] UserInfoQueryInput queryInput)
        {
            var data = await _adminService.LoadUserListAsync(queryInput);

            return new ResponseDto(data);
        }

        /// <summary>
        /// 禁用
        /// </summary>
        /// <param name="stateInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> UpdateUserStatus([FromBody] UpdateUserStateInput stateInput)
        {
            var data = await _adminService.UpdateUserStateAsync(stateInput);

            if (data != null)
            {
                _messageHandler.SendMessage(data);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 强制下线
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> ForceOffLine([FromBody] long userId)
        {
            var data = await _adminService.ForceOffLineAsync(userId);

            if (data != null)
            {
                _messageHandler.SendMessage(data);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseDto GetSysSetting()
        {
            var sys = RedisComponent.GetSysSetting();

            return new ResponseDto(sys);
        }

        /// <summary>
        /// 保存系统设置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseDto SaveSysSetting(SysSettingDto sysSettingDto)
        {
            RedisComponent.SetSysSetting(sysSettingDto);

            return new ResponseDto();
        }

        /// <summary>
        /// 加载会议列表
        /// </summary>
        /// <param name="meetingQueryInput"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadMeetingList(MeetingQueryInput meetingQueryInput)
        {
            var data = await _adminService.LoadMeetingListAsync(meetingQueryInput);

            return new ResponseDto(data);
        }
    }
}
