using Zol.Common;
using Zol.Common.Config;
using Zol.IGrains;
using Zol.OrleansWebApi;
using Zol.OrleansWebApi.IoC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Ray.Core;
using Ray.RabbitMQ;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebActivatorEx;
using Ray.IGrains;
using Ray.Core.Messaging;
using Ray.Core.Client;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]
namespace Zol.OrleansWebApi
{
    public class NinjectWebCommon
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }

        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                GlobalConfiguration.Configuration.DependencyResolver = new NinjectResolver(kernel);
                return kernel;
            }
            catch (Exception ex)
            {
                Zol.Common.Logger.Error("Create Kernel Fail", ex);
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static IClusterClient CreateClusterClient()
        {
            var client = new ClientBuilder()
                 .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ITestGrain).Assembly))
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
                 .ConfigureServices((servicecollection) =>
                 {
                     servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
                     servicecollection.AddRabbitMQ();//注册RabbitMq为默认消息队列
                     servicecollection.PostConfigure<RabbitConfig>(c =>
                     {
                         c.UserName = "scp";
                         c.Password = "scp";
                         c.Hosts = new[] { "10.1.1.36:5672" };
                         c.MaxPoolSize = 100;
                         c.VirtualHost = "test-host";
                     });
                 })
                 .Build();

            client.Connect(RetryFilter).GetAwaiter().GetResult();
            return client;

            async Task<bool> RetryFilter(Exception exception)
            {
                Logger.Error("Create Cluster Client Connect Fail", exception);
                await Task.Delay(TimeSpan.FromSeconds(2));
                return true;
            }
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IClusterClient>().ToConstant(CreateClusterClient());//.InRequestScope();

        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var servicecollection = new ServiceCollection();
                //SubManager.Parse(servicecollection, typeof(AccountCoreHandler).Assembly);//注册handle
                servicecollection.AddSingleton<IClientFactory, ClientFactory>();//注册Client获取方法
                servicecollection.AddSingleton<ISerializer, ProtobufSerializer>();//注册序列化组件
                servicecollection.AddRabbitMQ();//注册RabbitMq为默认消息队列
                servicecollection.PostConfigure<RabbitConfig>(c =>
                {
                    c.UserName = "scp";
                    c.Password = "scp";
                    c.Hosts = new[] { "10.1.1.36:5672" };
                    c.MaxPoolSize = 100;
                    c.VirtualHost = "test-host";
                });

                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error("", ex);
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
                        .UseLocalhostClustering()
                        //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IAccount).Assembly).WithReferences())
                        ;
                        return builder;
                    });
                    Console.WriteLine("Client successfully connect to silo host");
                    break;

                }
                catch (SiloUnavailableException ex)
                {
                    attempt++;
                    Logger.Error($"Attempt {attempt} of {initializeAttemptsBeforeFailing} failed to initialize the Orleans client.", ex);
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