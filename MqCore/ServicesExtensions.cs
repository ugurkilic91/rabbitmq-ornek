using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using System.Diagnostics;

namespace MqCore
{
    public static class ServicesExtensions
    {
        public static void AddMyLibraryServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnection>(p =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration.GetSection("MqUrl").Value,
                    UserName = configuration.GetSection("Name").Value,
                    Password = configuration.GetSection("Name").Value,
                    RequestedHeartbeat = TimeSpan.FromSeconds(10)
                };
                return factory.CreateConnection();
            });
            services.AddTransient<IModel>(p =>
            {
                var connection = p.GetService<IConnection>();
                return connection.CreateModel();
            });
            services.AddTransient<RetryPolicy>((p) =>
            {
                return Policy.Handle<Exception>().Retry(3, (exception, retryCount) =>
                {
                    Debug.WriteLine("3.Kez Hata Oldu Sayısı : "+ retryCount);
                    if (retryCount == 3)
                        Debug.WriteLine("3.Kez Hata Oldu");
                });
            });
            services.AddHostedService<ConsumeRabbitMQHostedService>();
            // register your services here
        }
    }
}