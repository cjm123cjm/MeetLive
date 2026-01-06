using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.Service.Implements;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    /// <summary>
    /// 更新
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly IAppUpdateService _appUpdateService;

        public UpdateController(IAppUpdateService appUpdateService)
        {
            _appUpdateService = appUpdateService;
        }

        /// <summary>
        /// 检测更新
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> CheckVersion([FromQuery] string version, [FromQuery] long userId)
        {
            var app = await _appUpdateService.GetLatestAppAsync(version, userId);

            if(app!=null&& app.FileType == 0)
            {

            }

            return new ResponseDto(app);
        }
    }
}
