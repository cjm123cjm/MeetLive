using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeetLive.Services.Api.Extensions;
using Serilog;
using MeetLive.Services.IService.Dtos;
using MeetLive.Services.Domain;
using MeetLive.Services.Api.Filters;
using MeetLive.Services.Api.Middlewares;
using MeetLive.Services.Service;
using IGeekFan.AspNetCore.Knife4jUI;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<TokenActionFilter>();
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
    options.SerializerSettings.ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    };
    // 针对 long 类型的自定义转换
    options.SerializerSettings.Converters.Add(new LongToStringConverterNewtonsoft());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule<AutofacModuleRegister>();
    });

//swagger添加jwt认证
builder.AddSwaggerAuth();

//认证
builder.AddAuthetication();

// 配置允许跨域
builder.AddCustomerCors();

//配置日志serilog
builder.Host.UseSerilog((context, logger) =>
{
    //Serilog读取配置
    logger.ReadFrom.Configuration(context.Configuration);
    logger.Enrich.FromLogContext();
});

//AutoMapper
builder.Services.AddAutoMapper(p =>
{
    p.AddMaps("MyCloudDrive.Service");
});

builder.Services.AddHttpContextAccessor();

//模型验证
builder.Services.AddOptions().Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errorInfo = new ValidationProblemDetails(context.ModelState).Errors
             .Select(t => $"{t.Key}:{string.Join(",", t.Value)}");
        return new OkObjectResult(new ResponseDto
        {
            Code = 400,
            Message = string.Join("\r\n", errorInfo)
        });
    };
});

//redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("redis");
    options.InstanceName = "myclonddrive_";
});

//注入dbcontext
builder.Services.AddDbContext<MeetLiveDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("mysql"), new MySqlServerVersion("5.7"));
});

//添加权限策略
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin", policy =>
    {
        policy.RequireClaim("IsAdmin", "True");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseKnife4UI();
}
LocationStorage.Instance = app.Services;

//错误中间件
app.UseErrorHandling();

app.UseCors("MeetLive.Client");

//认证
app.UseAuthentication();
//授权
app.UseAuthorization();

app.MapControllers();

app.Run();