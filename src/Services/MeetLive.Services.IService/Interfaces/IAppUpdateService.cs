using MeetLive.Services.Domain.Entities;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.IService.Interfaces
{
    public interface IAppUpdateService
    {
        /// <summary>
        /// 加载更新列表
        /// </summary>
        /// <param name="appUpdateQuery"></param>
        /// <returns></returns>
        Task<PageDto<AppUpdateDto>> LoadUpdateListAsync(AppUpdateQuery appUpdateQuery);

        /// <summary>
        /// 添加/修改
        /// </summary>
        /// <param name="appUpdate"></param>
        /// <returns></returns>
        Task SaveAppUpdateAsync(AppUpdateAddOrUpdateInput appUpdate);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="appUpdateId"></param>
        /// <returns></returns>
        Task DeleteAppUpdateAsync(long appUpdateId);

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="postUpdate"></param>
        /// <returns></returns>
        Task PostUpdateAppStatusAsync(PostUpdateAppStatusInput postUpdate);

        /// <summary>
        /// 获取最新的版本
        /// </summary>
        /// <returns></returns>
        Task<AppUpdateDto?> GetLatestAppAsync(string version, long userId);
    }
}
