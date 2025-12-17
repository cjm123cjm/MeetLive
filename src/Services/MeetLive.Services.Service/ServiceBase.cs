using AutoMapper;
using MeetLive.Services.Common.Snowflake;
using MeetLive.Services.IService.Dtos.Outputs;
using MeetLive.Services.IService.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MeetLive.Services.Service
{
    public class ServiceBase
    {
        protected long LoginUserId { get; set; }
        protected string LoginUserName { get; set; }
        protected string LoginUserEmail { get; set; }
        public bool IsAdmin { get; set; }
        public int? Sex { get; set; }
        public string? MeetingNo { get; set; }
        public string? CurrentMeetingId { get; set; }
        protected IMapper ObjectMapper { get; set; }
        protected string ServerUrl { get; set; }
        protected IdWorker SnowIdWorker { get; set; }
        protected FolderPath FolderPath { get; set; }
        public ServiceBase()
        {
            var httpContext = LocationStorage.Instance.GetService<IHttpContextAccessor>()!;

            if (httpContext.HttpContext != null && httpContext.HttpContext.User != null && httpContext.HttpContext.User.Identity != null)
            {
                if (httpContext.HttpContext!.User!.Identity!.IsAuthenticated)
                {
                    LoginUserId = Convert.ToInt64(httpContext.HttpContext.User.Claims.First(t => t.Type == "UserId").Value);
                    IsAdmin = Convert.ToBoolean(httpContext.HttpContext.User.Claims.First(t => t.Type == "IsAdmin").Value);
                    LoginUserEmail = httpContext.HttpContext.User.Claims.First(t => t.Type == "Email").Value.ToString();

                    var userDto = RedisComponent.GetUserInfoByUserId(LoginUserId.ToString())!;
                    MeetingNo = userDto.MeetingNo;
                    CurrentMeetingId = userDto.CurrentMeetingId;
                    LoginUserName = userDto.NickName;

                    var sex = httpContext.HttpContext.User.Claims.First(t => t.Type == "Sex").Value;
                    Sex = sex == null ? null : Convert.ToInt32(sex);
                }
                ServerUrl = $"{httpContext.HttpContext.Request.Scheme}://{httpContext.HttpContext.Request.Host}";
            }

            ObjectMapper = LocationStorage.Instance.GetService<IMapper>()!;

            FolderPath = LocationStorage.Instance.GetRequiredService<IOptions<FolderPath>>().Value;

            SnowIdWorker = SnowflakeUtil.CreateIdWorker();
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public UserInfoDto GetUserInfoDto()
        {
            return new UserInfoDto
            {
                UserId = LoginUserId,
                Email = LoginUserEmail,
                NickName = LoginUserName,
                Sex = Sex,
                IsAdmin = IsAdmin,
                MeetingNo = MeetingNo,
                CurrentMeetingId = CurrentMeetingId
            };
        }
    }
}
