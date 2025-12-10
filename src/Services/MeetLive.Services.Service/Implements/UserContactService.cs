using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.Repository;
using MeetLive.Services.Domain.UnitOfWork;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeetLive.Services.Service.Implements
{
    public class UserContactService : ServiceBase, IUserContactService
    {
        private readonly IUserContactRepository _userContactRepository;
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IUserContactApplyRepository _userContactApplyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserContactService(
            IUserContactRepository userContactRepository,
            IUserInfoRepository userInfoRepository,
            IUserContactApplyRepository userContactApplyRepository,
            IUnitOfWork unitOfWork)
        {
            _userContactRepository = userContactRepository;
            _userInfoRepository = userInfoRepository;
            _userContactApplyRepository = userContactApplyRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 根据邮箱查询用户
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<SearchContactDto?> SearchContactAsync(string email)
        {
            var userInfo = await _userInfoRepository.SelectByEmailAsync(email);
            if (userInfo == null)
            {
                return null;
            }

            SearchContactDto searchContactDto = new SearchContactDto
            {
                UserId = userInfo.UserId,
                NickName = userInfo.NickName
            };
            if (userInfo.UserId == LoginUserId)
            {
                searchContactDto.Status = -1;
                return searchContactDto;
            }

            //判断对方有没有将你拉黑
            //1.列表
            var userContact = await _userContactRepository.SelectByUserAndContactIdAsync(userInfo.UserId, LoginUserId);
            //2.申请列表
            var apply = await _userContactApplyRepository.SelectByApplyUserIdAndReceiveUserIdAsync(LoginUserId, userInfo.UserId);
            if ((apply != null && apply.Status == 3) || (userContact != null && userContact.Status == 3))
            {
                searchContactDto.Status = 3;
                return searchContactDto;
            }

            //待处理
            if (apply != null && apply.Status == 0)
            {
                searchContactDto.Status = 0;
                return searchContactDto;
            }

            var myUserContact = await _userContactRepository.SelectByUserAndContactIdAsync(LoginUserId, userInfo.UserId);
            if (userContact != null && userContact.Status == 1 && myUserContact != null && myUserContact.Status == 1)
            {
                searchContactDto.Status = 1;
                return searchContactDto;
            }

            return searchContactDto;
        }

        /// <summary>
        /// 申请好友
        /// </summary>
        /// <param name="userContactApply"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<int> SaveUserContactAsync(UserContactApply userContactApply)
        {
            //判断接收人是否拉黑了申请人
            var userContact = await _userContactRepository.SelectByUserAndContactIdAsync(userContactApply.ReceiveUserId, userContactApply.ApplyUserId);
            if (userContact != null && userContact.Status == 3)
            {
                throw new BusinessException("对方已将你拉黑");
            }

            //对方是你的好友,再申请就不走申请了,直接修改状态
            if (userContact != null && userContact.Status == 2)
            {
                var myUserContact = await _userContactRepository.SelectByUserAndContactIdAsync(LoginUserId, userContactApply.ReceiveUserId);
                if (myUserContact != null)
                {
                    myUserContact.Status = 1;
                    myUserContact.LastUpdateTime = DateTime.Now;

                    await _unitOfWork.SaveChangesAsync();

                    return 1;
                }
            }

            var apply = await _userContactApplyRepository.SelectByApplyUserIdAndReceiveUserIdAsync(userContactApply.ApplyUserId, userContactApply.ReceiveUserId);
            if (apply == null)
            {
                userContactApply.LastApplyTime = DateTime.Now;
                await _userContactApplyRepository.AddAsync(userContactApply);
            }
            else
            {
                apply.LastApplyTime = DateTime.Now;
                apply.Status = 0;
            }

            await _unitOfWork.SaveChangesAsync();

            return 0;
        }

        /// <summary>
        /// 处理申请消息
        /// </summary>
        /// <param name="applyAsyncInput"></param>
        /// <returns></returns>
        public async Task DealWithApplyAsync(DealWithApplyAsyncInput applyAsyncInput)
        {
            //查询有没有这个申请
            var apply = await _userContactApplyRepository.SelectByApplyUserIdAndReceiveUserIdAsync(applyAsyncInput.ApplyUserId, LoginUserId);
            if (apply == null)
            {
                throw new BusinessException("参数错误");
            }

            //同意
            if (applyAsyncInput.Status == 1)
            {
                //我添加对方
                UserContact userContact1 = new UserContact
                {
                    UserId = LoginUserId,
                    ContactId = applyAsyncInput.ApplyUserId,
                    Status = 1,
                    LastUpdateTime = DateTime.Now
                };
                await _userContactRepository.UpdateOrAddUserContact(userContact1);

                //对方添加我
                UserContact userContact2 = new UserContact
                {
                    UserId = applyAsyncInput.ApplyUserId,
                    ContactId = LoginUserId,
                    Status = 1,
                    LastUpdateTime = DateTime.Now
                };
                await _userContactRepository.UpdateOrAddUserContact(userContact2);
            }

            apply.Status = applyAsyncInput.Status;

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 加载联系人
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserContactDto>> LoadContactUserAsync()
        {
            var data = await (from a in _userContactRepository.QueryWhere(t => t.UserId == LoginUserId && t.Status == 1).OrderByDescending(t => t.LastUpdateTime)
                              join b in _userInfoRepository.Query().AsNoTracking() on a.ContactId equals b.UserId
                              select new UserContactDto
                              {
                                  ContactId = a.ContactId,
                                  NickName = b.NickName
                              }).ToListAsync();

            return data;
        }

        /// <summary>
        /// 加载申请信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserContactApplyDto>> LoadContactApplyAsync()
        {
            var applys = await _userContactApplyRepository
                                .QueryWhere(t => t.ReceiveUserId == LoginUserId)
                                .Select(t => new UserContactApplyDto
                                {
                                    ApplyUserId = t.ApplyUserId,
                                    LastApplyTime = t.LastApplyTime,
                                    Status = t.Status
                                })
                                .OrderByDescending(t => t.LastApplyTime)
                                .ToListAsync();

            var applyUserIds = applys.Select(t => t.ApplyUserId).Distinct().ToList();

            var userinfo = await _userInfoRepository.QueryWhere(t => applyUserIds.Contains(t.UserId)).ToListAsync();

            foreach (var item in applys)
            {
                item.ApplyNickName = userinfo.FirstOrDefault(t => t.UserId == item.ApplyUserId)!.NickName;
            }

            return applys;
        }

        /// <summary>
        /// 有多少条申请
        /// </summary>
        /// <returns></returns>
        public async Task<int> LoadContactApplyDealWithCountAsync()
        {
            return await _userContactApplyRepository.QueryWhere(t => t.ReceiveUserId == LoginUserId && t.Status == 0).CountAsync();
        }

        /// <summary>
        /// 删除/拉黑联系人
        /// </summary>
        /// <param name="deleteContactInput"></param>
        /// <returns></returns>
        public async Task DeleteContactAsync(DeleteContactInput deleteContactInput)
        {
            var userContact = await _userContactRepository.QueryWhere(t => t.UserId == LoginUserId &&
                                                                           t.ContactId == deleteContactInput.ContactId).FirstOrDefaultAsync();
            if (userContact == null)
            {
                throw new BusinessException("联系人不存在");
            }

            userContact.Status = deleteContactInput.Status;

            _userContactRepository.Update(userContact);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
