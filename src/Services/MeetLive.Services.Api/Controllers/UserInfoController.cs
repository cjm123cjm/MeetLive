using MeetLive.Services.Common.Captcha;
using MeetLive.Services.Common.RedisUtil;
using MeetLive.Services.Domain.CustomerException;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.IService.Dtos.Inputs;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.IService.Options;
using MeetLive.Services.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MeetLive.Services.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserInfoController : BaseController
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IOptions<FolderPath> _options;
        private readonly FFmpegUtils _ffmpegUtils;

        public UserInfoController(
            IUserInfoService userInfoService,
            IOptions<FolderPath> options,
            FFmpegUtils ffmpegUtils)
        {
            _userInfoService = userInfoService;
            _options = options;
            _ffmpegUtils = ffmpegUtils;
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

        /// <summary>
        /// 获取用户头像
        /// </summary>
        [HttpGet]
        public IActionResult GetUserAvatar()
        {
            // 1. 检查用户自定义头像
            string userAvatarPath = Path.Combine(_options.Value.PhysicalPath, "avatar", $"{LoginUserId}.jpg");
            if (System.IO.File.Exists(userAvatarPath))
            {
                return PhysicalFile(userAvatarPath, "image/jpeg");
            }

            // 2. 返回默认头像
            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            string defaultAvatarPath = Path.Combine(env.WebRootPath, "user.jpg");

            if (System.IO.File.Exists(defaultAvatarPath))
            {
                return PhysicalFile(defaultAvatarPath, "image/jpeg");
            }

            // 3. 都没有，返回404
            return NotFound("头像不存在");
        }

        /// <summary>
        /// 更新用户头像
        /// </summary>
        [HttpGet]
        public async Task<ResponseDto> UploadAvatar(IFormFile formFile)
        {
            try
            {
                await UploadImage(formFile);

                return new ResponseDto();
            }
            catch (Exception ex)
            {
                return new ResponseDto(false, ex.Message);
            }
        }

        /// <summary>
        /// 上传头像
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        [NonAction]
        private async Task UploadImage(IFormFile formFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(formFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new BusinessException("请上传图片类型的文件");
            }

            string folder = Path.Combine(_options.Value.PhysicalPath, "avatar");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            //将图片文件转成jpg类型
            string tempFileName = $"{LoginUserId}_temp{fileExtension}";
            string tempFilePath = Path.Combine(folder, tempFileName);
            // 保存上传的临时文件
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
            var tempFileInfo = new FileInfo(tempFilePath);
            string targetExtension = ".jpg";
            string targetFileName = $"{LoginUserId}{targetExtension}";
            string finalFilePath = Path.Combine(folder, targetFileName);

            _ffmpegUtils.TransferImageType(tempFileInfo, finalFilePath);

            //删除临时文件
            if (tempFileInfo.Exists)
            {
                tempFileInfo.Delete();
            }
        }

        /// <summary>
        /// 获取系统设置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseDto GetSysSetting()
        {
            var sys = RedisComponent.GetSysSetting();

            return new ResponseDto(sys);
        }

        /// <summary>
        /// 保存系统设置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseDto SaveSysSetting(SysSettingDto sysSettingDto)
        {
            RedisComponent.SetSysSetting(sysSettingDto);

            return new ResponseDto();
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="updateUserInfoInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> UpdateUserInfo([FromForm] UpdateUserInfoInput updateUserInfoInput)
        {
            await _userInfoService.UpdateUserInfoAsync(updateUserInfoInput);

            if (updateUserInfoInput.Avatar != null)
            {
                await UploadImage(updateUserInfoInput.Avatar);
            }

            return new ResponseDto();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="updatePasswordInput"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseDto> UpdatePassword(UpdatePasswordInput updatePasswordInput)
        {
            await _userInfoService.UpdatePasswordAsync(updatePasswordInput);

            return new ResponseDto();
        }
    }
}
