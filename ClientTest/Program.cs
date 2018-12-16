using Ray.IGrains;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;
using Orleans.Hosting;
using Zol.Common.Config;
using Ray.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;
using Ray.Core.Messaging;
using Ray.IGrains.Actors;
using Ray.Handler;
using Ray.Core;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray.Core.Client;

namespace ClientTest
{
    class Program
    {
        static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                using (var client = await StartClientWithRetries())
                {
                    //var handlerStartup = client.ServiceProvider.GetService<HandlerStartup>();
                    //await Task.WhenAll(
                    // handlerStartup.Start(SubscriberGroup.Core),
                    // handlerStartup.Start(SubscriberGroup.Db),
                    // handlerStartup.Start(SubscriberGroup.Rep));
                    while (true)
                    {
                        // var actor = client.GetGrain<IAccount>(0);
                        // Console.WriteLine("Press Enter for times...");
                        int currentAccount = 16;
                        Console.WriteLine($"添加前余额为{await client.GetGrain<IAccount>(currentAccount).GetBalance()}");
                        Console.WriteLine("start");
                        Console.ReadLine();
                        var length = 10000;// int.Parse(Console.ReadLine());
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        //await Task.WhenAll(Enumerable.Range(0, length).Select(x => client.GetGrain<IAccount>(currentAccount).AddAmount(1000)));

                        Task[] tsk = new Task[length];
                        Parallel.For(0, length, i =>
                        {
                            tsk[i] = client.GetGrain<IAccount>(currentAccount).AddAmount(1000);
                        });
                        await Task.WhenAll(tsk);

                        stopWatch.Stop();
                        Console.WriteLine($"{length }次操作完成，耗时:{stopWatch.ElapsedMilliseconds}ms");
                        await Task.Delay(200);
                        Console.WriteLine($"添加后余额为{await client.GetGrain<IAccountRep>(currentAccount).GetBalance()}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
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

                        //Ray
                        .ConfigureServices((context, servicecollection) =>
                        {
                            servicecollection.AddMQHandler();//注册所有handler
                            servicecollection.AddRay();//注册Client获取方法
                            servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
                            servicecollection.AddRabbitMQ();//注册RabbitMq为默认消息队列
                            servicecollection.AddLogging(logging => logging.AddConsole());
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
    }

}
