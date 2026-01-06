using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.UnitOfWork;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MeetLive.Services.Service.Implements
{
    public class AppUpdateService : ServiceBase, IAppUpdateService
    {
        private readonly IAppUpdateRepository _appUpdateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AppUpdateService(IAppUpdateRepository appUpdateRepository, IUnitOfWork unitOfWork)
        {
            _appUpdateRepository = appUpdateRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 加载更新列表
        /// </summary>
        /// <param name="appUpdateQuery"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PageDto<AppUpdateDto>> LoadUpdateListAsync(AppUpdateQuery appUpdateQuery)
        {
            var appQuery = _appUpdateRepository.Query().AsNoTracking();
            if (appUpdateQuery.StartTime != null)
            {
                appQuery = appQuery.Where(t => t.CreatedTime >= appUpdateQuery.StartTime.Value);
            }
            if (appUpdateQuery.EndTime != null)
            {
                appQuery = appQuery.Where(t => t.CreatedTime <= appUpdateQuery.EndTime.Value);
            }

            var total = await appQuery.CountAsync();

            PageDto<AppUpdateDto> pageDto = new PageDto<AppUpdateDto>
            {
                PageIndex = appUpdateQuery.PageIndex,
                PageSize = appUpdateQuery.PageSize,
                TotalCount = total,
            };

            var appData = await appQuery
                                .OrderByDescending(t => t.Id)
                                .Skip((appUpdateQuery.PageIndex - 1) * appUpdateQuery.PageSize)
                                .Take(appUpdateQuery.PageSize)
                                .ToListAsync();

            pageDto.Data = ObjectMapper.Map<List<AppUpdateDto>>(appData);

            return pageDto;
        }

        /// <summary>
        /// 添加/修改
        /// </summary>
        /// <param name="appUpdate"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task SaveAppUpdateAsync(AppUpdateAddOrUpdateInput appUpdate)
        {
            //版本号不能重复
            var versionAny = await _appUpdateRepository.QueryWhere(t => t.Id != appUpdate.Id && t.Version == appUpdate.Version).AnyAsync();
            if (versionAny)
            {
                throw new BusinessException("版本号已存在");
            }
            //判断版本号是否大于历史版本号
            var lastApp = await _appUpdateRepository.QueryWhere(t => t.Id != appUpdate.Id).OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            if (lastApp != null)
            {
                var last = Convert.ToInt64(lastApp.Version.Replace(".", ""));
                var current = Convert.ToInt64(appUpdate.Version.Replace(".", ""));
                if (current < last)
                {
                    throw new BusinessException("版本号要大于历史版本号");
                }
            }

            AppUpdate? app = null;
            if (appUpdate.Id == 0)
            {
                app = ObjectMapper.Map<AppUpdate>(appUpdate);

                await _appUpdateRepository.AddAsync(app);
            }
            else
            {
                app = await _appUpdateRepository.GetByIdAsync(appUpdate.Id);
                if (app == null)
                {
                    throw new BusinessException("参数错误");
                }
                if (app.Status != 0)
                {
                    throw new BusinessException("已发布,无法修改");
                }

                ObjectMapper.Map(appUpdate, app);
            }

            await _unitOfWork.SaveChangesAsync();

            //保存文件
            if (appUpdate.File != null)
            {
                var folder = Path.Combine(FolderPath.PhysicalPath, "app");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var saveFile = Path.Combine(folder, app.Id + ".exe");

                using var targetStream = System.IO.File.Create(saveFile);

                await appUpdate.File.CopyToAsync(targetStream);

                targetStream.Dispose();
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="appUpdateId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task DeleteAppUpdateAsync(long appUpdateId)
        {
            var app = await _appUpdateRepository.GetByIdAsync(appUpdateId);
            if (app == null)
            {
                throw new BusinessException("参数错误");
            }
            if (app.Status != 0)
            {
                throw new BusinessException("已发布,无法删除");
            }

            _appUpdateRepository.Delete(app);

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="postUpdate"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task PostUpdateAppStatusAsync(PostUpdateAppStatusInput postUpdate)
        {
            var app = await _appUpdateRepository.GetByIdAsync(postUpdate.Id);
            if (app == null)
            {
                throw new BusinessException("参数错误");
            }
            if (postUpdate.Status == 1 && string.IsNullOrWhiteSpace(postUpdate.GrayscaleId))
            {
                throw new BusinessException("灰度发布人不能为空");
            }
            app.Status = postUpdate.Status;
            app.GrayscaleId = postUpdate.GrayscaleId;

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 获取最新的版本号
        /// </summary>
        /// <param name="version"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<AppUpdateDto?> GetLatestAppAsync(string version, long userId)
        {
            var app = await _appUpdateRepository.QueryWhere(t =>
                       // 对于版本号字符串比较，需要特殊处理
                       CompareVersion(t.Version, version) > 0 &&
                       (t.Status == 2 ||
                        (t.Status == 1 && t.GrayscaleId != null && t.GrayscaleId.Contains(userId.ToString())))).FirstOrDefaultAsync();

            return ObjectMapper.Map<AppUpdateDto>(app);
        }

        // 版本号比较辅助方法
        private int CompareVersion(string version1, string version2)
        {
            if (version1 == null && version2 == null) return 0;
            if (version1 == null) return -1;
            if (version2 == null) return 1;

            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                var v1 = i < v1Parts.Length ? v1Parts[i] : 0;
                var v2 = i < v2Parts.Length ? v2Parts[i] : 0;

                if (v1 != v2)
                    return v1.CompareTo(v2);
            }

            return 0;
        }
    }
}
