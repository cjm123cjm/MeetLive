using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.UnitOfWork;
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
    public class AdminService : ServiceBase, IAdminService
    {
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMeetingInfoRepository _meetInfoRepository;

        public AdminService(IUserInfoRepository userInfoRepository, IUnitOfWork unitOfWork)
        {
            _userInfoRepository = userInfoRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="queryInput"></param>
        /// <returns></returns>
        public async Task<PageDto<UserInfoDto>> LoadUserListAsync(UserInfoQueryInput queryInput)
        {
            var query = _userInfoRepository.Query().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(queryInput.NickName))
            {
                query = query.Where(t => t.NickName.Contains(queryInput.NickName));
            }
            if (!string.IsNullOrWhiteSpace(queryInput.Email))
            {
                query = query.Where(t => t.Email.Contains(queryInput.Email));
            }

            PageDto<UserInfoDto> pageDto = new PageDto<UserInfoDto>();

            pageDto.TotalCount = await query.CountAsync();
            pageDto.PageSize = queryInput.PageSize;
            pageDto.PageIndex = queryInput.PageIndex;

            var user = await query.OrderByDescending(t => t.CreatedTime).Skip((queryInput.PageIndex - 1) * queryInput.PageSize).ToListAsync();

            pageDto.Data = ObjectMapper.Map<List<UserInfoDto>>(user);

            return pageDto;
        }

        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="stateInput"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<MessageSendDto<object>?> UpdateUserStateAsync(UpdateUserStateInput stateInput)
        {
            var user = await _userInfoRepository.GetByIdAsync(stateInput.UserId);
            if (user == null)
            {
                throw new BusinessException("参数错误");
            }
            user.Status = stateInput.Status;

            await _unitOfWork.SaveChangesAsync();

            if (user.Status == 0)
            {
                return await ForceOffLineAsync(user.UserId);
            }

            return null;
        }

        /// <summary>
        /// 强制下线
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MessageSendDto<object>?> ForceOffLineAsync(long userId)
        {
            var user = await _userInfoRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new BusinessException("参数错误");
            }

            MessageSendDto<object> messageSendDto = new MessageSendDto<object>();
            if (user.LastOffTime == null || user.LastLoginTime > user.LastOffTime)
            {
                messageSendDto.MessageSendType = MessageSendTypeEnum.USER;
                messageSendDto.MessageType = MessageTypeEnum.FORCE_OFF_LIVE;
                messageSendDto.ReceiveUserId = userId.ToString();

                RedisComponent.ClearUserInfo(userId.ToString());

                return messageSendDto;
            }

            return null;
        }

        /// <summary>
        /// 获取会议列表
        /// </summary>
        /// <param name="meetingQueryInput"></param>
        /// <returns></returns>
        public async Task<PageDto<MeetingInfoDto>> LoadMeetingListAsync(MeetingQueryInput meetingQueryInput)
        {
            var query = _meetInfoRepository.QueryWhere(t => t.Status == meetingQueryInput.Status);
            if (!string.IsNullOrWhiteSpace(meetingQueryInput.MeetingNo))
            {
                query = query.Where(t => t.MeetingNo.Contains(meetingQueryInput.MeetingNo));
            }
            if (!string.IsNullOrWhiteSpace(meetingQueryInput.MeetingName))
            {
                query = query.Where(t => t.MeetingName.Contains(meetingQueryInput.MeetingName));
            }

            var total = await query.CountAsync();

            PageDto<MeetingInfoDto> pageDto = new PageDto<MeetingInfoDto>
            {
                PageSize = meetingQueryInput.PageSize,
                PageIndex = meetingQueryInput.PageIndex,
                TotalCount = total
            };

            var data = await query.OrderByDescending(t => t.MeetingId).Skip((meetingQueryInput.PageIndex - 1) * meetingQueryInput.PageSize)
                  .Take(meetingQueryInput.PageSize).ToListAsync();

            pageDto.Data = ObjectMapper.Map<List<MeetingInfoDto>>(data);

            return pageDto;
        }
    }
}
