using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MeetLive.Services.Common;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.IService.Options;
using MeetLive.Services.IService.Dtos.Outputs;

namespace MeetLive.Services.Api.Filters
{
    /// <summary>
    /// token 自动刷新
    /// </summary>
    public class TokenActionFilter : IAsyncActionFilter
    {
        private readonly IJwtTokenGenerator _tokenService;
        private readonly JwtOptions _jwtTokenOption;

        /// <summary>
        /// 注入服务
        /// </summary>
        /// <param name="tokenService"></param>
        /// <param name="jwtTokenOption"></param>
        public TokenActionFilter(IJwtTokenGenerator tokenService, IOptionsMonitor<JwtOptions> jwtTokenOption)
        {
            _tokenService = tokenService;
            _jwtTokenOption = jwtTokenOption.CurrentValue;
        }

        // 判断是否含有Authorize
        private bool HasIAuthorize(ActionExecutingContext context)
        {
            if (context.Filters.Any(filter => filter is IAuthorizationFilter))
            {
                return true;
            }
            // 终节点：里面包含了路由方法的所有元素信息（特性等信息）
            var endpoint = context.HttpContext.GetEndpoint();
            return endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null;
        }

        /// <summary>
        /// Action 执行前后
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();

            // 只对需要认证授权的方法刷新token
            if (!HasIAuthorize(context))
            {
                return;
            }
            if (context.HttpContext.User.Identity is { IsAuthenticated: true })
            {
                var objectResult = context.Result as ObjectResult;
                var val = objectResult?.Value;
                if (val == null)
                {
                    return;
                }
                var type = objectResult!.DeclaredType; // 实际返回的类型
                var userClaims = context.HttpContext.User.Claims.ToList();
                // 到期时间
                var exp = Convert.ToInt64(userClaims.FirstOrDefault(p => p.Type == "exp")!.Value);
                // 判断token 是否过期: 拿到期时间-当前过期 = token 剩余可用时间
                var timeSpan = DateTimeUtil.GetDataTime(exp).Subtract(DateTime.Now);
                // 剩余的时间
                var minutes = timeSpan.TotalMinutes;
                // 如果剩余时间少于Token有效时间的一半，我们返回一个新的Token给前端
                if (minutes < _jwtTokenOption.Expires / 2.0)
                {
                    var token = _tokenService.GenerateToken(new UserInfoDto
                    {
                        NickName = userClaims.FirstOrDefault(p => p.Type == "NickName")!.Value,
                        UserId = Convert.ToInt64(userClaims.FirstOrDefault(p => p.Type == "UserId")!.Value),
                        Email = userClaims.FirstOrDefault(p => p.Type == "Email")!.Value,
                        IsAdmin = Convert.ToBoolean(userClaims.FirstOrDefault(p => p.Type == "IsAdmin")!.Value),
                    });
                    // 设置新的token，给前端重新存储
                    type!.GetProperty("Token")!.SetValue(val, token);
                    context.Result = new JsonResult(val);
                }
            }
        }
    }
}
