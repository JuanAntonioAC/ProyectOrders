
using RabbitMQ.Client;
using Shared.RabbitMQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMQ
{
    class RabbitMqPublisher : IRabbitMqPublisher
    {

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher(string hostName = "localhost")
        {
            var factory = new ConnectionFactory { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        public void Publish(string queueName, string message)
        {
            _channel.QueueDeclare(queue: queueName,
                           durable: false,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  basicProperties: null,
                                  body: body);
        }
    }
}
