using MeetLive.Services.Common;
using MeetLive.Services.Common.Captcha;
using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.Domain.Entities;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.UnitOfWork;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MeetLive.Services.Service.Implements
{
    public class UserInfoService : ServiceBase, IUserInfoService
    {
        private readonly IUserInfoRepository _userInfoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IConfiguration _configuration;
        public UserInfoService(
            IUserInfoRepository userInfoRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IConfiguration configuration)
        {
            _userInfoRepository = userInfoRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _configuration = configuration;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="registerInput"></param>
        /// <returns></returns>
        public async Task RegisterAsync(RegisterInput registerInput)
        {
            var email = await _userInfoRepository.SelectByEmailAsync(registerInput.Email);
            if (email != null)
            {
                throw new BusinessException("邮箱已存在");
            }

            UserInfo userInfo = new UserInfo
            {
                UserId = SnowIdWorker.NextId(),
                Email = registerInput.Email,
                NickName = registerInput.NickName,
                Password = MD5Util.MD5Encrypt(registerInput.Password),
                MeetingNo = CreateCaptcha.CreateCharCode(10),
                LastOffTime = DateTime.UtcNow,
                Status = 1
            };

            await _userInfoRepository.AddAsync(userInfo);

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginInput"></param>
        /// <returns></returns>
        public async Task<LoginResponseDto> LoginAsync(LoginInput loginInput)
        {
            var user = await _userInfoRepository.SelectByEmailAsync(loginInput.Email);
            if (user == null || user.Password != loginInput.Password)
            {
                throw new BusinessException("账户或者密码不正确");
            }

            if (user.Status == 0)
            {
                throw new BusinessException("账户已禁用");
            }

            if (user.LastLoginTime != null && user.LastOffTime <= user.LastLoginTime)
            {
                throw new BusinessException("账户已在其它地方登录,请退出后再登录");
            }

            var userDto = ObjectMapper.Map<UserInfoDto>(user);

            string[] adminEmail = _configuration.GetValue<string>("AdminEmail").Split(",");
            if (adminEmail.Contains(userDto.Email))
            {
                userDto.IsAdmin = true;
            }

            var token = _jwtTokenGenerator.GenerateToken(userDto);

            //数据存到redis里
            RedisComponent.SetUserInfo(userDto, token);

            return new LoginResponseDto
            {
                Token = token,
                User = userDto
            };
        }
    }
}
