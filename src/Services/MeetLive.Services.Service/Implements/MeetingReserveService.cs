using MeetLive.Services.Common.Captcha;
using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.UnitOfWork;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetLive.Services.Service.Implements
{
    public class MeetingReserveService : ServiceBase, IMeetingReserveService
    {
        private readonly IMeetingReserveRepository _reserveRepository;
        private readonly IMeetingReserveMemberRepository _meetingReserveMemberRepository;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MeetingReserveService(
            IMeetingReserveRepository reserveRepository,
            IMeetingReserveMemberRepository meetingReserveMemberRepository,
            IUserInfoRepository userInfoRepository,
            IUnitOfWork unitOfWork)
        {
            _reserveRepository = reserveRepository;
            _meetingReserveMemberRepository = meetingReserveMemberRepository;
            _userInfoRepository = userInfoRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 加载我的预约会议
        /// </summary>
        /// <returns></returns>
        public async Task<List<MeetingReserveDto>> LoadMyMeetingReserveAsync()
        {
            var meetingIds = await _meetingReserveMemberRepository.QueryWhere(t => t.InviteUserId == LoginUserId).Select(t => t.MeetingId).ToListAsync();

            var reserves = await _reserveRepository
                                 .QueryWhere(t => meetingIds.Contains(t.MeetingId) && t.Status == 1)
                                 .OrderBy(t => t.StartTime)
                                 .ToListAsync();

            //查询人员
            var userId = reserves.Select(t => t.CreatedUserId).Distinct().ToList();

            var data = ObjectMapper.Map<List<MeetingReserveDto>>(reserves);

            var userData = await _userInfoRepository.QueryWhere(t => userId.Contains(t.UserId)).ToListAsync();

            foreach (var item in data)
            {
                item.CreatedUserName = userData.FirstOrDefault(t => t.UserId == item.CreatedUserId)?.NickName;
            }

            return data;
        }

        /// <summary>
        /// 创建预约会议
        /// </summary>
        /// <param name="meetingReserveInput"></param>
        /// <returns></returns>
        public async Task CreateMeetingReserveAsync(MeetingReserveInput meetingReserveInput)
        {
            var model = ObjectMapper.Map<MeetingReserve>(meetingReserveInput);

            model.MeetingId = SnowIdWorker.NextId();
            model.MeetingNo = CreateCaptcha.CreateNumCode(10);
            model.Status = 0;
            model.CreatedUserId = LoginUserId;
            model.CreatedTime = DateTime.Now;

            await _reserveRepository.AddAsync(model);

            List<MeetingReserveMember> reserveMembers = new List<MeetingReserveMember>();
            //添加预约会议人
            MeetingReserveMember reserveMember = new MeetingReserveMember
            {
                MeetingId = model.MeetingId,
                InviteUserId = LoginUserId
            };
            reserveMembers.Add(reserveMember);

            if (!string.IsNullOrEmpty(meetingReserveInput.InviteUserIds))
            {
                var userIds = meetingReserveInput.InviteUserIds.Split(',');
                foreach (var userId in userIds)
                {
                    reserveMembers.Add(new MeetingReserveMember
                    {
                        MeetingId = model.MeetingId,
                        InviteUserId = Convert.ToInt64(userId)
                    });
                }
            }

            await _meetingReserveMemberRepository.AddAsync(reserveMembers.ToArray());

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 删除预约会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public async Task DeleteMeetingReserveAsync(string meetingId)
        {
            var meetingReserve = await _reserveRepository.QueryWhere(t => t.MeetingId.ToString() == meetingId && t.CreatedUserId == LoginUserId).FirstOrDefaultAsync();
            if (meetingReserve != null)
            {
                _reserveRepository.Delete(meetingReserve);

                //删除预约会议人员表
                var members = await _meetingReserveMemberRepository.QueryWhere(t => t.MeetingId.ToString() == meetingId).ToListAsync();
                if (members.Any())
                {
                    _meetingReserveMemberRepository.Delete(members.ToArray());
                }

                await _unitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 参加人删除会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task DeleteMeetingReserveByUserAsync(string meetingId)
        {
            var meetingReserve = await _reserveRepository.GetByIdAsync(Convert.ToInt64(meetingId));
            if (meetingReserve == null)
            {
                throw new BusinessException("参数错误");
            }
            if (meetingReserve.CreatedUserId == LoginUserId)
            {
                await DeleteMeetingReserveAsync(meetingId);
            }
            else
            {
                //删除预约会议人员表
                var member = await _meetingReserveMemberRepository.QueryWhere(t => t.MeetingId.ToString() == meetingId && t.InviteUserId == LoginUserId).FirstOrDefaultAsync();
                if (member != null)
                {
                    _meetingReserveMemberRepository.Delete(member);

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// 加载当天要参加的会议
        /// </summary>
        /// <returns></returns>
        public async Task<List<MeetingReserveDto>> LoadTodayMeetingAsync()
        {
            var meetingIds = await _meetingReserveMemberRepository.QueryWhere(t => t.InviteUserId == LoginUserId).Select(t => t.MeetingId).ToListAsync();

            DateTime start = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime end = start.AddDays(1);

            var reserves = await _reserveRepository
                                 .QueryWhere(t => meetingIds.Contains(t.MeetingId) && t.Status == 1 && t.StartTime >= start && t.StartTime < end)
                                 .OrderBy(t => t.StartTime)
                                 .ToListAsync();

            //查询人员
            var userId = reserves.Select(t => t.CreatedUserId).Distinct().ToList();

            var data = ObjectMapper.Map<List<MeetingReserveDto>>(reserves);

            var userData = await _userInfoRepository.QueryWhere(t => userId.Contains(t.UserId)).ToListAsync();

            foreach (var item in data)
            {
                item.CreatedUserName = userData.FirstOrDefault(t => t.UserId == item.CreatedUserId)?.NickName;
            }

            return data;
        }
    }
}
