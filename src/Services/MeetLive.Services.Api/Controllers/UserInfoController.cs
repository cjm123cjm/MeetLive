using MeetLive.Services.Common.Captcha;
using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly IUserInfoService _userInfoService;

        public UserInfoController(IUserInfoService userInfoService)
        {
            _userInfoService = userInfoService;
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseDto CheckCode()
        {
            VerifyCode codeInfo = CreateCaptcha.CreateVerifyCode(4, VerifyCodeType.ARITH);

            CacheManager.Set(RedisKeyPrefix.VerifyCode + codeInfo.CodeKey, codeInfo, TimeSpan.FromMinutes(5));

            return new ResponseDto
            {
                IsSuccess = true,
                Result = new { codeInfo.Image, codeInfo.CodeKey }
            };
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="registerInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> Register(RegisterInput registerInput)
        {
            try
            {
                //校验验证码
                var codeInfo = CacheManager.Get<VerifyCode>(RedisKeyPrefix.VerifyCode + registerInput.CodeKey);
                if (codeInfo == null || codeInfo.Code != registerInput.Code)
                {
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "验证码错误"
                    };
                }

                //注册
                await _userInfoService.RegisterAsync(registerInput);

                return new ResponseDto();
            }
            finally
            {
                CacheManager.Remove(RedisKeyPrefix.VerifyCode + registerInput.CodeKey);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> Login(LoginInput loginInput)
        {
            try
            {
                //校验验证码
                var codeInfo = CacheManager.Get<VerifyCode>(RedisKeyPrefix.VerifyCode + loginInput.CodeKey);
                if (codeInfo == null || codeInfo.Code != loginInput.Code)
                {
                    return new ResponseDto
                    {
                        IsSuccess = false,
                        Message = "验证码错误"
                    };
                }

                var data = await _userInfoService.LoginAsync(loginInput);

                return new ResponseDto(data);
            }
            finally
            {
                CacheManager.Remove(RedisKeyPrefix.VerifyCode + loginInput.CodeKey);
            }
        }
    }
}
