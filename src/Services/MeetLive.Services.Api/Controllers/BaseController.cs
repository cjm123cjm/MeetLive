using Microsoft.AspNetCore.Mvc;

namespace MeetLive.Services.Api.Controllers
{
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// 当前用户登录ID
        /// </summary>
        public long LoginUserId
        {
            get
            {
                if (HttpContext.User.Identity is { IsAuthenticated: true })
                {
                    return Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(p => p.Type.Equals("UserId"))!.Value);
                }
                return 0;
            }
        }
    }
}
