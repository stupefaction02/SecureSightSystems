using SecureSightSystems.Core;
using SecureSightSystems.Core.Services;
using SecureSightSystems.Services;
using SecureSightSystems.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace SecureSightSystems.Infrastructure
{
    public class DependencyContainer
    {
        private static readonly ServiceCollection services = new ServiceCollection();

        static DependencyContainer()
        {
            services.ConfigureNetwork();
            services.ConfigureLoggerFactory();

            services.AddSingleton<IApiClient, WebApiClient>();
            services.AddSingleton<ApplicationInfoManager>();
            services.AddSingleton<FrameController>();
            services.AddSingleton<Navigation>();
            services.AddSingleton<OverviewViewModel>();
            services.AddSingleton<MainViewModel>();

            serviceProvider = services.BuildServiceProvider();
        }

        public static ServiceProvider serviceProvider { get; }

        public static T Resolve<T>() => serviceProvider.GetRequiredService<T>();
    }

    public static class ServicesExtrensions
    {
        public static void ConfigureNetwork(this IServiceCollection services)
        {
            services.AddHttpClient("Default", client => {
                client.BaseAddress = new Uri(WebApiClient.BaseUrl);
            });
        }

        public static void ConfigureLoggerFactory(this IServiceCollection services)
        {
            var lf = LoggerFactory.Create((builder) =>
            {
                builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
                builder.AddSimpleConsole(x => x.SingleLine = true).SetMinimumLevel(LogLevel.Trace);
            });

            services.AddSingleton(lf);
        }
    }
}
