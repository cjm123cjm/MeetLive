using Castle.DynamicProxy;
using MeetLive.Services.Common;
using Newtonsoft.Json;
using System.Reflection;

namespace MeetLive.Services.Api.Extensions
{
    /// <summary>
    /// 服务AOP
    /// </summary>
    public class ServiceAop : IInterceptor
    {
        private readonly ILogger<ServiceAop> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServiceAop(
            ILogger<ServiceAop> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 实例化IInterceptor唯一方法 
        /// </summary>
        /// <param name="invocation">包含被拦截方法的信息</param>
        public void Intercept(IInvocation invocation)
        {
            string json;
            try
            {
                json = JsonConvert.SerializeObject(invocation.Arguments);
            }
            catch (Exception ex)
            {
                json = "无法序列化，可能是兰姆达表达式等原因造成，按照框架优化代码" + ex.ToString();
            }

            DateTime startTime = DateTime.Now;
            AopLogInfo apiLogAopInfo = new AopLogInfo
            {
                RequestTime = startTime.ToString("yyyy-MM-dd hh:mm:ss fff"),
                OpUserName = "",
                RequestMethodName = invocation.Method.Name,
                RequestParamsName = string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()),
                RequestParamsData = json,
            };

            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.User != null && _httpContextAccessor.HttpContext.User.Identity != null)
            {
                if (_httpContextAccessor.HttpContext!.User!.Identity!.IsAuthenticated)
                {
                    apiLogAopInfo.OpUserId = Convert.ToInt64(_httpContextAccessor.HttpContext.User.Claims.First(t => t.Type == "UserId").Value);
                    apiLogAopInfo.OpUserName = _httpContextAccessor.HttpContext.User.Claims.First(t => t.Type == "NickName").Value.ToString();
                }
            }

            try
            {
                //在被拦截的方法执行完毕后 继续执行当前方法，注意是被拦截的是异步的
                invocation.Proceed();


                // 异步获取异常，先执行
                if (IsAsyncMethod(invocation.Method))
                {

                    //Wait task execution and modify return value
                    if (invocation.Method.ReturnType == typeof(Task))
                    {
                        invocation.ReturnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally(
                            (Task)invocation.ReturnValue,
                            async () => await SuccessAction(invocation, apiLogAopInfo, startTime), /*成功时执行*/
                            ex =>
                            {
                                LogEx(ex, apiLogAopInfo);
                            });
                    }
                    //Task<TResult>
                    else
                    {
                        invocation.ReturnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                            invocation.Method.ReturnType.GenericTypeArguments[0],
                            invocation.ReturnValue,
                            async (o) => await SuccessAction(invocation, apiLogAopInfo, startTime, o), /*成功时执行*/
                            ex =>
                            {
                                LogEx(ex, apiLogAopInfo);
                            });
                    }

                }
                else
                {
                    // 同步1
                    string jsonResult;
                    try
                    {
                        jsonResult = JsonConvert.SerializeObject(invocation.ReturnValue);
                    }
                    catch (Exception ex)
                    {
                        jsonResult = "无法序列化，可能是兰姆达表达式等原因造成，按照框架优化代码" + ex.ToString();
                    }

                    DateTime endTime = DateTime.Now;
                    string ResponseTime = (endTime - startTime).Milliseconds.ToString();
                    apiLogAopInfo.ResponseTime = endTime.ToString("yyyy-MM-dd hh:mm:ss fff");
                    apiLogAopInfo.ResponseIntervalTime = ResponseTime + "ms";
                    apiLogAopInfo.ResponseJsonData = jsonResult;
                    Console.WriteLine(JsonConvert.SerializeObject(apiLogAopInfo));
                }
            }
            catch (Exception ex)
            {
                LogEx(ex, apiLogAopInfo);
                throw;
            }
        }

        private async Task SuccessAction(IInvocation invocation, AopLogInfo apiLogAopInfo, DateTime startTime, object? o = null)
        {
            DateTime endTime = DateTime.Now;
            string ResponseTime = (endTime - startTime).Milliseconds.ToString();
            apiLogAopInfo.ResponseTime = endTime.ToString("yyyy-MM-dd hh:mm:ss fff");
            apiLogAopInfo.ResponseIntervalTime = ResponseTime + "ms";
            apiLogAopInfo.ResponseJsonData = JsonConvert.SerializeObject(o);

            await Task.Run(() =>
            {
                //写入日志
                Console.WriteLine("执行成功-->" + JsonConvert.SerializeObject(apiLogAopInfo));
                _logger.LogInformation("执行成功-->" + JsonConvert.SerializeObject(apiLogAopInfo));
            });
        }

        private void LogEx(Exception ex, AopLogInfo dataIntercept)
        {
            if (ex != null)
            {
                //写入日志
                Console.WriteLine("error!!!:" + ex.Message + JsonConvert.SerializeObject(dataIntercept));
                _logger.LogError("error!!!:" + ex.Message + JsonConvert.SerializeObject(dataIntercept));
            }
        }


        public static bool IsAsyncMethod(MethodInfo method)
        {
            return
                method.ReturnType == typeof(Task) ||
                method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        }
    }

    internal static class InternalAsyncHelper
    {
        public static async Task AwaitTaskWithPostActionAndFinally(Task actualReturnValue, Func<Task> postAction, Action<Exception> finalAction)
        {
            Exception exception = null;

            try
            {
                await actualReturnValue;
                await postAction();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static async Task<T> AwaitTaskWithPostActionAndFinallyAndGetResult<T>(Task<T> actualReturnValue, Func<object, Task> postAction,
        Action<Exception> finalAction)
        {
            Exception exception = null;
            try
            {
                var result = await actualReturnValue;
                await postAction(result);
                return result;
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                finalAction(exception);
            }
        }

        public static object? CallAwaitTaskWithPostActionAndFinallyAndGetResult(Type taskReturnType, object actualReturnValue,
             Func<object, Task> action, Action<Exception> finalAction)
        {
            return typeof(InternalAsyncHelper)
                .GetMethod("AwaitTaskWithPostActionAndFinallyAndGetResult", BindingFlags.Public | BindingFlags.Static)
                ?.MakeGenericMethod(taskReturnType)
                .Invoke(null, new object[] { actualReturnValue, action, finalAction });
        }
    }
}
