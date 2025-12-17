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
using MeetLive.Services.WebSocket;
using MeetLive.Services.WebSocket.Message;
using DotNetCore.CAP;
using MeetLive.Services.Common;
using MeetLive.Services.IService.Options;

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
            Code = 200,
            IsSuccess = false,
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

//添加websocket
builder.Services.AddSingleton<ChannelContextUtils>();
builder.Services.AddSingleton<HeartBeatHandler>();
builder.Services.AddSingleton<NettyWebScoketServer>();
builder.Services.AddSingleton<TokenValidationHandler>();
builder.Services.AddSingleton<WebSocketHandler>();

// 注册具体的实现类
builder.Services.AddScoped<RedisMessageHandler>();
builder.Services.AddScoped<CapRabbitMQMessageHandler>();
builder.Services.AddScoped<IMessageHandler>(scope =>
{
    var configuration = scope.GetRequiredService<IConfiguration>();

    string? messageChannel = configuration.GetValue<string>("MessageChannel");

    return messageChannel?.ToLower() switch
    {
        "rabbitmq" => scope.GetRequiredService<CapRabbitMQMessageHandler>(),
        "redis" => scope.GetRequiredService<RedisMessageHandler>(),
        _ => throw new InvalidOperationException($"不支持的消息通道: {messageChannel}")
    };
});

// 注册 Hosted Service
builder.Services.AddHostedService<MessageHostedService>();

//配置RabbitMq
var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
builder.Services.Configure<RabbitMQOptions>(rabbitConfig);
var rabbitOptions = rabbitConfig.Get<RabbitMQOptions>()!;
builder.Services.AddCap(setup =>
{
    setup.UseMySql(builder.Configuration.GetConnectionString("mysql") ?? string.Empty);
    setup.UseEntityFramework<MeetLiveDbContext>();
    setup.UseRabbitMQ(mq =>
    {
        mq.HostName = rabbitOptions.HostName;
        mq.VirtualHost = rabbitOptions.VirtualHost;
        mq.UserName = rabbitOptions.UserName;
        mq.Password = rabbitOptions.Password;
        mq.Port = rabbitOptions.Port;

        // 交换机配置 - 使用 Fanout
        mq.ExchangeName = "meetlive.cap.fanout.exchange";
    });
    //仪表盘默认的访问地址是：http://localhost:xxx/cap，你可以在d.MatchPath配置项中修改cap路径后缀为其他的名字。
    setup.UseDashboard();// 注册仪表盘

    //重试
    setup.FailedRetryCount = 3;
    //间隔10s
    setup.FailedRetryInterval = 10;
});

//注册上传文件配置
builder.Services.Configure<FolderPath>(builder.Configuration.GetSection("FolderPath"));

//打印分表sql语句
TableSplitUtils.GetSplitTableSql();


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

//注册静态文件
app.UseStaticFiles();

//认证
app.UseAuthentication();
//授权
app.UseAuthorization();

app.MapControllers();

app.Run();