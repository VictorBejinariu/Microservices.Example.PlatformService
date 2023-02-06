using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration??throw new ArgumentNullException(nameof(configuration));
            var factory = new ConnectionFactory(){
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };
            
            try
            {
                _connection = factory.CreateConnection();     
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange:"trigger", type:ExchangeType.Fanout);
                _connection.ConnectionShutdown+= RabbitMQ_ConnectionShoutDown;

                System.Console.WriteLine("Connected to Messsage Bus");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"--> Could not connect to MessageBus : {ex.Message}");
            }

        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if(_connection.IsOpen){
                System.Console.WriteLine("--> RabbitMQ Connection Open, sending message. Fantastic");
                SendMessage(message);
            }
            else
            {
                System.Console.WriteLine("--> RabitMQ Connection is closed");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                exchange:"trigger", 
                routingKey: "",
                basicProperties: null,
                body:body);

            System.Console.WriteLine($"--> We sent {message}");
        }
        
        private void RabbitMQ_ConnectionShoutDown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine($"--> RabbitMQ Connection shutdown");
        }

        public void Dispose(){
            System.Console.WriteLine("Message Bus Disposed");
            if(_channel.IsOpen){
                _channel.Close();
                _connection.Close();
            }
        }
    }
}