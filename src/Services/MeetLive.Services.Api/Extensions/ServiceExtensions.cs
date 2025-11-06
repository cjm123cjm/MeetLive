using MeetLive.Services.IService.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace MeetLive.Services.Api.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// 认证
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static void AddAuthetication(this WebApplicationBuilder builder)
        {
            //配置jwt
            var jwtOption = builder.Configuration.GetSection("JwtOptions");
            builder.Services.Configure<JwtOptions>(jwtOption);

            // 添加认证服务
            JwtOptions jwtTokenOption = jwtOption.Get<JwtOptions>();
            var key = Encoding.ASCII.GetBytes(jwtTokenOption.Secret);
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtTokenOption.Issuer,
                        ValidAudience = jwtTokenOption.Audience,
                        ValidateAudience = true
                    };
                });
            builder.Services.AddAuthorization();
        }

        /// <summary>
        /// 配置跨域
        /// </summary>
        /// <param name="builder"></param>
        public static void AddCustomerCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(p =>
            {
                p.AddPolicy("MeetLive.Client", p =>
                {
                    p.AllowAnyHeader();
                    // 这里需要改成WithOrigins()方法,填写你实际的客户端地址
                    p.SetIsOriginAllowed(p => true);
                    p.AllowAnyMethod();
                    p.AllowCredentials(); // 主要是为了允许signalR跨域通讯
                });
            });
        }

        /// <summary>
        /// swagger添加jwt认证
        /// </summary>
        /// <param name="builder"></param>
        public static void AddSwaggerAuth(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(p =>
            {
                p.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Contact = new()
                    {
                        Email = "2671433141@qq.com",
                        Name = "MeetLive",
                        Url = new Uri("http://baidu.com")
                    },
                    Description = "MeetLive",
                    Title = "MeetLive"
                });


                //Bearer 的scheme定义
                var securityScheme = new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization 头使用Bearer 体系方案. 例: \"Authorization: Bearer 你的token\"",
                    Name = "Authorization",
                    //参数添加在头部
                    In = ParameterLocation.Header,
                    //使用Authorize头部
                    Type = SecuritySchemeType.Http,
                    //内容为以 bearer开头
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                };

                //把所有方法配置为增加bearer头部信息
                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "JWT认证"
                            }
                        },
                        new string[] {}
                    }
                };

                //注册到swagger中
                p.AddSecurityDefinition("JWT认证", securityScheme);
                p.AddSecurityRequirement(securityRequirement);


                // 加载xml文档注释
                p.IncludeXmlComments(AppContext.BaseDirectory + Assembly.GetExecutingAssembly().GetName().Name + ".xml", true);
                // 实体层的注释也需要加上
                p.IncludeXmlComments(AppContext.BaseDirectory + "MeetLive.Services.IService");

            });
        }
    }
}
