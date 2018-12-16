using Zol.Common;
using Zol.Common.Config;
using Ray.Grain;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Ray.RabbitMQ;
using System.Threading.Tasks;
using Ray.IGrains;
using Ray.MongoDB;
using Ray.Handler;
using Microsoft.Extensions.Logging;
using Ray.Core.Messaging;
using Ray.Core;
using System;
using Orleans.Runtime;
using Ray.Core.Client;
using Ray.IGrains.Actors;

namespace Zol.Core
{
    public class SiloWrapper : Singleton<SiloWrapper>
    {
        public SiloWrapper()
        {
            var invariant = AppConfigHelper.Invariant;// "Oracle.DataAccess.Client";// "System.Data.SqlClient";
            string connectionString = AppConfigHelper.ConnectionString;

            _siloHost = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = AppConfigHelper.ClusterId;
                    options.ServiceId = AppConfigHelper.ServiceId;
                })
                .UseAdoNetClustering(options =>
                {
                    options.ConnectionString = connectionString;
                    options.Invariant = invariant;
                })
               //.UseAdoNetReminderService(options =>
               //{
               //    options.Invariant = invariant;
               //    options.ConnectionString = connectionString;
               //})
               //.AddAdoNetGrainStorage("GrainStorageForPro", options =>
               //{
               //    options.Invariant = invariant;
               //    options.ConnectionString = connectionString;
               //})
               //.EnableDirectClient()//单个Host可以直接使用SiloHost的Client,不需要再用ClientBuilder建Client了
               .UseDashboard(options =>
               {
                   options.Host = "*";
                   options.Port = 7080;
                   options.HostSelf = true;
                   options.CounterUpdateIntervalMs = 1000;
               })
               .ConfigureEndpoints(siloPort: AppConfigHelper.SiloPort, gatewayPort: AppConfigHelper.GatewayPort, listenOnAnyHostAddress: true)
               .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Account).Assembly).WithReferences())
               //.UseTransactions()//事务配置
               .ConfigureLogging(logging =>
               {
                   logging.SetMinimumLevel(LogLevel.Error);
                   logging.AddConsole();
               })

               //Ray
               .ConfigureServices((context, servicecollection) =>
               {
                   servicecollection.AddRay();
                   servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
                   servicecollection.AddMongoDbSiloGrain();//注册mongodb为事件存储库
                   servicecollection.AddMQHandler();//注册所有handler
               })

               .Configure<MongoConfig>(c =>
               {
                   c.Connection = "mongodb://127.0.0.1:27017";
               })
               .Configure<RabbitConfig>(c =>
               {
                   c.UserName = "scpmq";
                   c.Password = "scpmq";
                   c.Hosts = new[] { "127.0.0.1:5672" };
                   c.MaxPoolSize = 100;
                   c.VirtualHost = "test-host";
               })
              .Build();

            //Client = _siloHost.Services.GetRequiredService<IClusterClient>();//把sliohost的IClusterClient暴露出去。

        }

        private static async Task<IClusterClient> StartClientWithRetries(int initializeAttemptsBeforeFailing = 5)
        {
            int attempt = 0;
            IClusterClient client;
            while (true)
            {
                try
                {
                    client = await ClientFactory.Build(() =>
                    {
                        var builder = new ClientBuilder()
                       .Configure<ClusterOptions>(options =>
                       {
                           options.ClusterId = AppConfigHelper.ClusterId;
                           options.ServiceId = AppConfigHelper.ServiceId;
                       })
                       .UseAdoNetClustering(options =>
                       {
                           options.ConnectionString = AppConfigHelper.ConnectionString;
                           options.Invariant = AppConfigHelper.Invariant;
                       })
                        .ConfigureServices((context, servicecollection) =>
                        {
                            servicecollection.AddRay();
                            servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
                            servicecollection.AddRabbitMQ();//注册RabbitMq为默认消息队列
                            servicecollection.AddLogging(logging => logging.AddConsole());
                            servicecollection.AddMQHandler();//注册所有handler
                            servicecollection.PostConfigure<RabbitConfig>(c =>
                            {
                                c.UserName = "scpmq";
                                c.Password = "scpmq";
                                c.Hosts = new[] { "127.0.0.1:5672" };
                                c.MaxPoolSize = 100;
                                c.VirtualHost = "test-host";
                            });
                        })
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccount).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole());
                        return builder;
                    });
                    Console.WriteLine("Client successfully connect to silo host");
                    break;
                }
                catch (SiloUnavailableException)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.");
                    if (attempt > initializeAttemptsBeforeFailing)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(4));
                }
            }

            return client;
        }

        public async Task<bool> StartAsync()
        {
            await _siloHost.StartAsync();
            Client = await StartClientWithRetries();
            var handlerStartup = Client.ServiceProvider.GetService<HandlerStartup>();
            await Task.WhenAll(
             handlerStartup.Start(SubscriberGroup.Core),
             handlerStartup.Start(SubscriberGroup.Db),
             handlerStartup.Start(SubscriberGroup.Rep));
            return true;
        }

        public async Task StopAsync()
        {
            Client.Dispose();
            await _siloHost.StopAsync();
        }

        readonly ISiloHost _siloHost;
        public IClusterClient Client;
    }
}
