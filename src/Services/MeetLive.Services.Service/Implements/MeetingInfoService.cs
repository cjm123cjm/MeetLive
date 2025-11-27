using MeetLive.Services.Common.Captcha;
using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.UnitOfWork;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Enums;
using MeetLive.Services.IService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MeetLive.Services.Service.Implements
{
    public class MeetingInfoService : ServiceBase, IMeetingInfoService
    {
        private readonly IMeetingInfoRepository _meetInfoRepository;
        private readonly IMeetingMemberRepository _meetMemberRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public MeetingInfoService(
            IMeetingInfoRepository meetInfoRepository,
            IMeetingMemberRepository meetMemberRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IUnitOfWork unitOfWork)
        {
            _meetInfoRepository = meetInfoRepository;
            _meetMemberRepository = meetMemberRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 查询会议
        /// </summary>
        /// <param name="queryInput"></param>
        /// <returns></returns>
        public async Task<PageDto<MeetingInfoDto>> LoadMeetingAsync(MeetingQueryInput queryInput)
        {
            PageDto<MeetingInfoDto> pageDto = new PageDto<MeetingInfoDto>
            {
                PageIndex = queryInput.PageIndex,
                PageSize = queryInput.PageSize,
            };

            var meetingIds = await _meetMemberRepository.QueryWhere(t => t.UserId == LoginUserId && t.Status == 1).Select(t => t.MeetingId).ToListAsync();

            pageDto.TotalCount = meetingIds.Count;

            var query = _meetInfoRepository.QueryWhere(t => meetingIds.Contains(t.MeetingId) && t.Status == queryInput.Status);
            if (!string.IsNullOrWhiteSpace(queryInput.MeetingNo))
            {
                query = query.Where(t => t.MeetingNo.Contains(queryInput.MeetingNo));
            }
            if (!string.IsNullOrWhiteSpace(queryInput.MeetingName))
            {
                query = query.Where(t => t.MeetingName.Contains(queryInput.MeetingName));
            }

            var meetingInfos = await query.OrderByDescending(t => t.CreatedTime)
                                          .Skip((queryInput.PageIndex - 1) * queryInput.PageSize)
                                          .Take(queryInput.PageSize)
                                          .ToListAsync();

            pageDto.Data = ObjectMapper.Map<List<MeetingInfoDto>>(meetingInfos);

            return pageDto;
        }

        /// <summary>
        /// 创建快速会议
        /// </summary>
        /// <param name="meetingInput"></param>
        /// <returns></returns>
        public async Task<string> QuickMeetingAsync(QuickMeetingInput meetingInput)
        {
            if (!string.IsNullOrWhiteSpace(CurrentMeetingId))
            {
                throw new BusinessException("你有未结束的会议,无法创建新会议");
            }

            MeetingInfo meetingInfo = new MeetingInfo
            {
                MeetingId = SnowIdWorker.NextId(),
                MeetingNo = meetingInput.MeetingType == 0 ? MeetingNo : CreateCaptcha.CreateCharCode(10),
                MeetingName = meetingInput.MeetingName,
                CreatedUserId = LoginUserId,
                JoinType = meetingInput.JoinType,
                JoinPassword = meetingInput.JoinPassword,
                StartTime = DateTime.Now,
                Status = 1
            };

            await _meetInfoRepository.AddAsync(meetingInfo);

            CurrentMeetingId = meetingInfo.MeetingId.ToString();

            var userDto = GetUserInfoDto();
            var token = _jwtTokenGenerator.GenerateToken(userDto);

            //数据存到redis里
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token + token, userDto, TimeSpan.FromDays(1));
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token_UserId + userDto.UserId, userDto, TimeSpan.FromDays(1));

            await _unitOfWork.SaveChangesAsync();

            return token;
        }

        /// <summary>
        /// 加入会议
        /// </summary>
        /// <param name="videoOpen">是否打开摄像头</param>
        /// <returns></returns>
        public async Task<MessageSendDto<object>> JoinMeetingAsync(bool videoOpen)
        {
            if (string.IsNullOrWhiteSpace(CurrentMeetingId))
            {
                throw new BusinessException("参数错误");
            }

            var meetingInfo = await _meetInfoRepository.GetByIdAsync(Convert.ToInt64(CurrentMeetingId));
            if (meetingInfo == null || meetingInfo.Status == 0)
            {
                throw new BusinessException("参数错误");
            }

            //校验用户是否被拉黑
            var checkUser = await _meetMemberRepository.QueryWhere(t => t.MeetingId == meetingInfo.MeetingId && t.UserId == LoginUserId).FirstOrDefaultAsync();
            if (checkUser != null && checkUser.Status == 4)
            {
                throw new BusinessException("你已经被拉黑");
            }

            //加入成员
            var memberType = meetingInfo.CreatedUserId == LoginUserId ? 0 : 1;
            await AddMeetingMember(meetingInfo.MeetingId, LoginUserId, LoginUserName, memberType);

            await _unitOfWork.SaveChangesAsync();

            //加入会议
            AddToMeeting(meetingInfo.MeetingId, LoginUserId, LoginUserName, Sex, memberType, videoOpen);

            //加入ws房间
            //发送ws消息
            MeetingJoinDto meetingJoinDto = new MeetingJoinDto
            {
                NewMember = CacheManager.HashGet<MeetingMemberDto>(RedisKeyPrefix.Redis_Key_Meeting_Root + meetingInfo.MeetingId, LoginUserId.ToString()),
                MeetingMemberList = CacheManager.HashValues<MeetingMemberDto>(RedisKeyPrefix.Redis_Key_Meeting_Root + meetingInfo.MeetingId).ToList()
            };
            MessageSendDto<object> messageSendDto = new MessageSendDto<object>
            {
                MessageType = (int)MessageTypeEnum.ADD_MEETING_ROOM,
                MeetingId = meetingInfo.MeetingId.ToString(),
                MessageSendType = MessageSendTypeEnum.GROUP,
                MessageContent = meetingJoinDto
            };

            return messageSendDto;
        }

        /// <summary>
        /// 加入成员
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <param name="userId">用户id</param>
        /// <param name="nickName">昵称</param>
        /// <param name="memberType">成员类型:0-主持人,1-普通成员</param>
        /// <returns></returns>
        public async Task AddMeetingMember(long meetingId, long userId, string nickName, int memberType)
        {
            var model = await _meetMemberRepository.QueryWhere(t => t.MeetingId == meetingId && t.UserId == userId, true).FirstOrDefaultAsync();

            if (model == null)
            {
                MeetingMember meetingMember = new MeetingMember
                {
                    MeetingId = meetingId,
                    UserId = userId,
                    NickName = nickName,
                    LastJoinTime = DateTime.Now,
                    MemberType = memberType,
                    Status = 1,
                    MeetingStatus = 1
                };

                await _meetMemberRepository.AddAsync(meetingMember);
            }
            else
            {
                model.NickName = nickName;
                model.LastJoinTime = DateTime.Now;
                model.MemberType = memberType;
                model.Status = 1;
                model.MeetingStatus = 1;
            }
        }

        /// <summary>
        /// 加入会议
        /// </summary>
        /// <param name="meetingId">会议id</param>
        /// <param name="userId">用户id</param>
        /// <param name="nickName">昵称</param>
        /// <param name="sex">性别</param>
        /// <param name="memberType">成员类型:0-主持人,1-普通成员</param>
        /// <param name="videoOpen">是否打开摄像头</param>
        /// <returns></returns>
        public void AddToMeeting(long meetingId, long userId, string nickName, int? sex, int memberType, bool videoOpen)
        {
            MeetingMemberDto meetingMemberDto = new MeetingMemberDto
            {
                MeetingId = meetingId,
                UserId = userId,
                NickName = nickName,
                LastJoinTime = DateTime.Now,
                MemberType = memberType,
                Status = 1,
                Sex = sex,
                VideoOpen = videoOpen
            };

            CacheManager.HashSet(RedisKeyPrefix.Redis_Key_Meeting_Root + meetingId.ToString(), userId.ToString(), meetingMemberDto);
        }

        /// <summary>
        /// 预加入会议
        /// </summary>
        /// <param name="meetingInput"></param>
        /// <returns></returns>
        public async Task<string> PreJoinMeetingAsync(PreJoinMeetingInput meetingInput)
        {
            var meetingInfo = await _meetInfoRepository
                                    .QueryWhere(t => t.MeetingNo == meetingInput.MeetingNo)
                                    .OrderByDescending(t => t.CreatedTime)
                                    .FirstOrDefaultAsync();
            if (meetingInfo == null)
            {
                throw new BusinessException("会议不存在");
            }

            if (meetingInfo.Status == 0)
            {
                throw new BusinessException("会议已结束");
            }

            if (!string.IsNullOrWhiteSpace(CurrentMeetingId) && CurrentMeetingId != meetingInfo.MeetingId.ToString())
            {
                throw new BusinessException("您有未结束的会议,无法加入其它会议");
            }

            //校验用户是否被拉黑
            var checkUser = await _meetMemberRepository.QueryWhere(t => t.MeetingId == meetingInfo.MeetingId && t.UserId == LoginUserId).FirstOrDefaultAsync();
            if (checkUser != null && checkUser.Status == 4)
            {
                throw new BusinessException("你已经被拉黑");
            }

            //判断要不要密码
            if (meetingInfo.JoinType == 0)
            {
                if (meetingInfo.JoinPassword != meetingInput.JoinPassword)
                {
                    throw new BusinessException("会议密码错误");
                }
            }

            CurrentMeetingId = meetingInfo.MeetingId.ToString();
            LoginUserName = meetingInput.NickName;

            var userDto = GetUserInfoDto();
            var token = _jwtTokenGenerator.GenerateToken(userDto);

            //数据存到redis里
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token + token, userDto, TimeSpan.FromDays(1));
            CacheManager.Set(RedisKeyPrefix.Redis_Key_Ws_Token_UserId + userDto.UserId, userDto, TimeSpan.FromDays(1));

            return token;
        }
    }
}
