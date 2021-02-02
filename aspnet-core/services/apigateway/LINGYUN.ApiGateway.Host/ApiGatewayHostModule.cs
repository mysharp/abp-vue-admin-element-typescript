﻿using DotNetCore.CAP;
using LINGYUN.Abp.EventBus.CAP;
using LINGYUN.ApiGateway.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.Repository;
using Ocelot.DependencyInjection;
using Ocelot.Extenssions;
using Ocelot.Middleware.Multiplexer;
using Ocelot.Provider.Polly;
using StackExchange.Redis;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Security.Claims;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.IdentityModel;
using Volo.Abp.Json;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Security.Encryption;
using Volo.Abp.VirtualFileSystem;

namespace LINGYUN.ApiGateway
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AbpHttpClientIdentityModelModule),
        typeof(AbpCachingStackExchangeRedisModule),
        typeof(AbpAutoMapperModule),
        typeof(ApiGatewayHttpApiClientModule),
        typeof(AbpCAPEventBusModule),
        typeof(AbpAspNetCoreModule)
        )]
    public class ApiGatewayHostModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();

            // 不启用则使用本地配置文件的方式启动Ocelot
            if (configuration.GetValue<bool>("EnabledDynamicOcelot"))
            {
                context.Services.AddSingleton<IFileConfigurationRepository, ApiHttpClientFileConfigurationRepository>();
            }

            PreConfigure<CapOptions>(options =>
            {
                options
                .UseSqlite("Data Source=./event-bus-cap.db")
                .UseDashboard()
                .UseRabbitMQ(rabbitMQOptions =>
                {
                    configuration.GetSection("CAP:RabbitMQ").Bind(rabbitMQOptions);
                });
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();
            
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddProfile<ApiGatewayMapperProfile>(validate: true);
            });

            Configure<ApiGatewayOptions>(configuration.GetSection("ApiGateway"));

            // 解决某些不支持类型的序列化
            Configure<AbpJsonOptions>(options =>
            {
                options.UseHybridSerializer = true;
            });
            // 中文序列化的编码问题
            Configure<AbpSystemTextJsonSerializerOptions>(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });

            context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AuthServer:Authority"];
                    options.RequireHttpsMetadata = false;
                    options.Audience = configuration["AuthServer:ApiName"];
                });

            Configure<AbpDistributedCacheOptions>(options =>
            {
                // 最好统一命名,不然某个缓存变动其他应用服务有例外发生
                options.KeyPrefix = "LINGYUN.Abp.Application";
                // 滑动过期30天
                options.GlobalCacheEntryOptions.SlidingExpiration = TimeSpan.FromDays(30);
                // 绝对过期60天
                options.GlobalCacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
            });

            Configure<RedisCacheOptions>(options =>
            {
                var redisConfig = ConfigurationOptions.Parse(options.Configuration);
                options.ConfigurationOptions = redisConfig;
                options.InstanceName = configuration["Redis:InstanceName"];
            });

            // 加解密
            Configure<AbpStringEncryptionOptions>(options =>
            {
                var encryptionConfiguration = configuration.GetSection("Encryption");
                if (encryptionConfiguration.Exists())
                {
                    options.DefaultPassPhrase = encryptionConfiguration["PassPhrase"] ?? options.DefaultPassPhrase;
                    options.DefaultSalt = encryptionConfiguration.GetSection("Salt").Exists()
                        ? Encoding.ASCII.GetBytes(encryptionConfiguration["Salt"])
                        : options.DefaultSalt;
                    options.InitVectorBytes = encryptionConfiguration.GetSection("InitVector").Exists()
                        ? Encoding.ASCII.GetBytes(encryptionConfiguration["InitVector"])
                        : options.InitVectorBytes;
                }
            });

            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<ApiGatewayHostModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("en", "en", "English"));
                options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));

                options.Resources
                    .Get<ApiGatewayResource>()
                    .AddVirtualJson("/Localization/Host");
            });

            var mvcBuilder = context.Services.AddMvc();
            mvcBuilder.AddApplicationPart(typeof(ApiGatewayHostModule).Assembly);

            Configure<AbpEndpointRouterOptions>(options =>
            {
                options.EndpointConfigureActions.Add(endpointContext =>
                {
                    endpointContext.Endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
                    endpointContext.Endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                });
            });

            if (!hostingEnvironment.IsDevelopment())
            {
                // Ssl证书
                var sslOptions = configuration.GetSection("App:SslOptions");
                if (sslOptions.Exists())
                {
                    var fileName = sslOptions["FileName"];
                    var password = sslOptions["Password"];
                    Configure<KestrelServerOptions>(options =>
                    {
                        options.ConfigureEndpointDefaults(cfg =>
                        {
                            cfg.UseHttps(fileName, password);
                        });
                    });
                }

                var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
                context.Services
                    .AddDataProtection()
                    .PersistKeysToStackExchangeRedis(redis, "ApiGatewayHost-Protection-Keys");
            }

            context.Services
                .AddOcelot()
                .AddPolly()
                .AddSingletonDefinedAggregator<AbpApiDefinitionAggregator>();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();

            // 网关不需要加代理中间件

            app.UseAuditing();
            app.UseVirtualFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAbpClaimsMap();
            app.MapWhen(
                ctx => ctx.Request.Path.ToString().StartsWith("/api/ApiGateway/Basic/"),
                appNext =>
                {
                    // 仅针对属于网关自己的控制器进入MVC管道
                    appNext.UseRouting();
                    appNext.UseConfiguredEndpoints();
                });
            // 启用ws协议
            app.UseWebSockets();
            app.UseOcelot().Wait();
        }
    }
}
