using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Shared.Models;
using System.Text;
using System.Text.Json;
using CustomerService.Data;
using Microsoft.EntityFrameworkCore;
using Azure.Core;

namespace CustomerService.Validator
{
    public class CustomerValidationConsumer : BackgroundService
    {
        #region Private Properties
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        #endregion

        #region Lifecycle
        public CustomerValidationConsumer(IServiceProvider serviceProvider)
        {

            _serviceProvider = serviceProvider;
          

            var factory = new ConnectionFactory { HostName = "localhost", DispatchConsumersAsync = true };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare("customer_validate", durable: false, exclusive: false, autoDelete: false);
        }

        /// <summary>
        /// Background task that listens to 'customer_validate' queue,
        /// validates the customer, and sends a response to the reply queue.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var request = JsonSerializer.Deserialize<CustomerValidationRequest>(message);

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

                    var customerExists = await db.Customers.AnyAsync(c => c.Id == request.CustomerId);

                    var response = new CustomerValidationResponse
                    {
                        CorrelationId = Guid.Parse(ea.BasicProperties.CorrelationId),
                        Exists = customerExists,
                        Message = customerExists ? null : "Cliente no encontrado"
                    };

                    var responseQueue = $"customer_response_{request.CorrelationId}";
                    _channel.QueueDeclare(responseQueue, durable: false, exclusive: false, autoDelete: true);

                    var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                    _channel.BasicPublish(exchange: "", routingKey: responseQueue, basicProperties: null, body: responseBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ExecuteAsync CustomerValidationConsumer");
                }
            };

            _channel.BasicConsume("customer_validate", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
        #endregion
    }

}
