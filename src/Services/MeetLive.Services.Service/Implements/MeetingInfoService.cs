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
        public async Task QuickMeetingAsync(QuickMeetingInput meetingInput)
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

            //数据存到redis里
            RedisComponent.UpdateUserInfoByUserId(userDto);

            await _unitOfWork.SaveChangesAsync();
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
                NewMember = RedisComponent.GetMeetingMember(meetingInfo.MeetingId.ToString(), LoginUserId.ToString()),
                MeetingMemberList = RedisComponent.GetMeetingMemberList(meetingInfo.MeetingId.ToString())
            };
            MessageSendDto<object> messageSendDto = new MessageSendDto<object>
            {
                MessageType = MessageTypeEnum.ADD_MEETING_ROOM,
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

            RedisComponent.SetMeetingMember(meetingId.ToString(), userId.ToString(), meetingMemberDto);
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
            RedisComponent.SetUserInfo(userDto, token);

            return token;
        }

        /// <summary>
        /// 退出会议
        /// </summary>
        /// <param name="type">2-退出会议,3-被踢出会议,4-被拉黑</param>
        /// <returns></returns>
        public async Task<MessageSendDto<object>?> ExitMeetingAsync(UserInfoDto userDto, int type)
        {
            if (string.IsNullOrWhiteSpace(userDto.CurrentMeetingId)) return null;

            string meetingId = userDto.CurrentMeetingId;
            var memberDto = RedisComponent.GetMeetingMember(meetingId, userDto.UserId.ToString());
            if (memberDto != null)
            {
                memberDto.Status = type;
                RedisComponent.SetMeetingMember(meetingId, userDto.UserId.ToString(), memberDto);
            }
            else
            {
                //还没参加就退出了
                userDto.CurrentMeetingId = null;

                //数据存到redis里
                RedisComponent.UpdateUserInfoByUserId(userDto);

                return null;
            }

            userDto.CurrentMeetingId = null;

            //数据存到redis里
            RedisComponent.UpdateUserInfoByUserId(userDto);

            //设置发送消息体
            var members = RedisComponent.GetMeetingMemberList(meetingId);
            MeetingExitDto meetingExitDto = new MeetingExitDto
            {
                ExitUserId = LoginUserId,
                MeetingMemberList = members,
                ExitStatus = type
            };
            MessageSendDto<object> messageSendDto = new MessageSendDto<object>
            {
                MessageType = MessageTypeEnum.EXIT_MEETING_ROOM,
                MessageContent = meetingExitDto,
                MeetingId = meetingId,
                MessageSendType = MessageSendTypeEnum.GROUP
            };

            //会议正常人员
            int onLiveCount = members.Where(t => t.Status == 1).Count();
            if (onLiveCount == 0)
            {
                //结束会议
                await FinishMeetingAsync(meetingId);
            }
            else
            {
                //被踢出会议、被拉黑
                if (type == 3 || type == 4)
                {
                    var currentMember = await _meetMemberRepository.QueryWhere(t => t.MeetingId.ToString() == meetingId && t.UserId == userDto.UserId, true).FirstOrDefaultAsync();
                    if (currentMember != null)
                    {
                        currentMember.Status = type;

                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }

            return messageSendDto;
        }

        /// <summary>
        /// 强制退出
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MessageSendDto<object>?> ForceExitMeetingAsync(int type, string userId)
        {
            var meetInfo = await _meetInfoRepository.QueryWhere(t => t.MeetingId.ToString() == CurrentMeetingId).FirstOrDefaultAsync();
            if (meetInfo == null || meetInfo.CreatedUserId != LoginUserId)
            {
                throw new BusinessException("参数错误");
            }
            UserInfoDto? userDto = RedisComponent.GetUserInfoByUserId(userId);
            if (userDto == null)
            {
                throw new BusinessException("参数错误");
            }

            return await ExitMeetingAsync(userDto, type);
        }

        /// <summary>
        /// 获取正在进行的会议
        /// </summary>
        /// <returns></returns>
        public async Task<MeetingInfoDto> GetCurrentMeetingAsync()
        {
            var meetingInfo = await _meetInfoRepository.GetByIdAsync(Convert.ToInt64(CurrentMeetingId));

            return ObjectMapper.Map<MeetingInfoDto>(meetingInfo);
        }

        /// <summary>
        /// 结束会议
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task<MessageSendDto<object>> FinishMeetingAsync(string meetingId)
        {
            if (string.IsNullOrWhiteSpace(meetingId))
            {
                throw new BusinessException("参数错误");
            }

            MeetingInfo? meeting = await _meetInfoRepository.GetByIdAsync(Convert.ToInt64(meetingId));
            if (meeting == null)
            {
                throw new BusinessException("参数错误");
            }
            if (meeting.CreatedUserId != LoginUserId && !IsAdmin)
            {
                throw new BusinessException("没有权限");
            }

            meeting.Status = 0;
            meeting.EndTime = DateTime.Now;

            //更新成员
            var meetmerber = await _meetMemberRepository.QueryWhere(t => t.MeetingId == meeting.MeetingId, true).ToListAsync();
            foreach (var item in meetmerber)
            {
                item.MeetingStatus = 0;
            }

            await _unitOfWork.SaveChangesAsync();

            //更新预约会议状态

            var meetingMembers = RedisComponent.GetMeetingMemberList(meeting.MeetingId.ToString());
            foreach (var item in meetingMembers)
            {
                var userDto = RedisComponent.GetUserInfoByUserId(item.UserId.ToString());
                if (userDto != null)
                {
                    userDto.CurrentMeetingId = "";
                    RedisComponent.UpdateUserInfoByUserId(userDto);
                }
            }
            RedisComponent.RemoveAllMeetingMember(meetingId);

            //发送消息
            MessageSendDto<object> messageSendDto = new MessageSendDto<object>
            {
                MessageSendType = MessageSendTypeEnum.GROUP,
                MessageType = MessageTypeEnum.FINIS_MEETING,
                MeetingId = meeting.MeetingId.ToString(),
            };

            return messageSendDto;
        }

        /// <summary>
        /// 删除会议记录
        /// </summary>
        /// <returns></returns>
        public async Task DeleteMeetingRecordAsync(string meetingId)
        {
            var memberRecord = await _meetMemberRepository.QueryWhere(t => t.MeetingId.ToString() == meetingId && t.UserId == LoginUserId, true).ToListAsync();

            foreach (var item in memberRecord)
            {
                item.Status = 0;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 加载参加会议的成员
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public async Task<List<MeetingMemberDto>> LoadMeetingMemberAsync(string meetingId)
        {
            var data = await _meetMemberRepository.QueryWhere(t => t.MeetingId.ToString() == meetingId).ToListAsync();

            return ObjectMapper.Map<List<MeetingMemberDto>>(data);
        }
    }
}
