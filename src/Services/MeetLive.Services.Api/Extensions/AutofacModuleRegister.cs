using Autofac;
using Autofac.Extras.DynamicProxy;
using MeetLive.Services.Domain.IRepository;
using MeetLive.Services.Domain.Repository;
using MeetLive.Services.IService.Interfaces;
using MeetLive.Services.Service.Implements;
using System.Reflection;

namespace MeetLive.Services.Api.Extensions
{
    /// <summary>
    /// autofac
    /// </summary>
    public class AutofacModuleRegister : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JwtTokenGenerator>().As<IJwtTokenGenerator>();

            var aopType = new List<Type> { typeof(ServiceAop) };
            builder.RegisterType<ServiceAop>();

            //获取 Service.dll 程序集服务,并注册
            builder.RegisterAssemblyTypes(Assembly.Load("MeetLive.Services.IService"), Assembly.Load("MeetLive.Services.Service"))
                .Where(a => a.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .EnableInterfaceInterceptors()
                .InterceptedBy(aopType.ToArray());

            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)); //注册仓储

            //获取 Repository.dll 程序集服务,并注册
            builder.RegisterAssemblyTypes(Assembly.Load("MeetLive.Services.Domain"))
                .Where(a => a.Name.EndsWith("Repository"))
                .AsImplementedInterfaces();
        }
    }
}
