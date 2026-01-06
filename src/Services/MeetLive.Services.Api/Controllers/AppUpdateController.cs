using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MeetLive.Services.Domain.Entities;

namespace MeetLive.Services.Api.Controllers
{
    /// <summary>
    /// 应用更新
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppUpdateController : ControllerBase
    {
        private readonly IAppUpdateService _appUpdateService;

        public AppUpdateController(IAppUpdateService appUpdateService)
        {
            _appUpdateService = appUpdateService;
        }

        /// <summary>
        /// 加载更新列表
        /// </summary>
        /// <param name="appUpdateQuery"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseDto> LoadUpdateList([FromQuery] AppUpdateQuery appUpdateQuery)
        {
            var data = await _appUpdateService.LoadUpdateListAsync(appUpdateQuery);

            return new ResponseDto(data);
        }

        /// <summary>
        /// 添加/修改
        /// </summary>
        /// <param name="appUpdate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> SaveAppUpdate([FromForm] AppUpdateAddOrUpdateInput appUpdate)
        {
            await _appUpdateService.SaveAppUpdateAsync(appUpdate);

            return new ResponseDto();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="appUpdateId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> DeleteAppUpdate([FromBody] long appUpdateId)
        {
            await _appUpdateService.DeleteAppUpdateAsync(appUpdateId);

            return new ResponseDto();
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="postUpdate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> PostUpdateAppStatus([FromBody] PostUpdateAppStatusInput postUpdate)
        {
            await _appUpdateService.PostUpdateAppStatusAsync(postUpdate);

            return new ResponseDto();
        }

    }
}
