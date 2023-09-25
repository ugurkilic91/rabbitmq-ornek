using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Polly.Retry;

namespace MqCore
{
    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        //private readonly ILogger _logger;
        //private IConnection _connection;
        private readonly IModel _channel;
        private readonly RetryPolicy _retryPolicy;

        public ConsumeRabbitMQHostedService(IModel channel, RetryPolicy retryPolicy)
        {
            _channel = channel;
            InitRabbitMQ();
            _retryPolicy = retryPolicy;
        }


        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            // create connection  
            //_connection = factory.CreateConnection();

            // create channel  
            //_channel = _connection.CreateModel();

            _channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);
            //_channel.QueueDeclare("product", false, false, false, null);
            _channel.QueueDeclare("product", exclusive: false);
            _channel.QueueBind("product", "demo-direct-exchange", "product");
            _channel.BasicQos(0, 1, false);

            //_connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                try
                {
                    _retryPolicy.Execute(() =>
                      { 
                          var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                          Debug.WriteLine(content);
                          if (content == "u1")
                          {
                              throw new Exception("Hata");
                          }

                      });
                }
                catch (Exception e)
                {
                    Debug.WriteLine("catch");
                }
                finally
                {
                    Debug.WriteLine("Final");
                    Thread.Sleep(10000);
                    _channel.BasicAck(ea.DeliveryTag, false);//Kuyruktan Sil

                }

            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("product", false, consumer);
            return Task.CompletedTask;
        }
        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            base.Dispose();
        }
    }
}
