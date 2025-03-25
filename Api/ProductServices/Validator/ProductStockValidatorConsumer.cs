using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using ProductServices.Data;
using Shared.Models;
using Microsoft.EntityFrameworkCore;
using Azure.Core;

namespace ProductServices.Validator
{
    public class ProductStockValidatorConsumer : BackgroundService
    {
        #region Private Properties
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;
        #endregion


        #region LifeCycle Methods
        /// <summary>
        /// Constructor ProductStockValidatorConsumer
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ProductStockValidatorConsumer(IServiceProvider serviceProvider)
        {
           
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory { HostName = "localhost" , DispatchConsumersAsync = true };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare("product_validate", durable: false, exclusive: false, autoDelete: false);
        }

        /// <summary>
        /// Execute Background service
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected  override  Task  ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<ProductStockValidationRequest>(message);

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
                var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);

                var response = new ProductStockValidationResponse
                {
                    CorrelationId = Guid.Parse(ea.BasicProperties.CorrelationId),
                    IsAvailable = product != null && product.Stock >= request.Quantity,
                    Message = product == null ? "Producto no encontrado" : null
                };

                var responseQueue = $"product_response_{request.CorrelationId}";

                _channel.QueueDeclare(responseQueue, durable: false, exclusive: false, autoDelete: true);
                var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

                _channel.BasicPublish(exchange: "", routingKey: responseQueue, basicProperties: null, body: responseBody);
            };

            _channel.BasicConsume("product_validate", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Dispose object
        /// </summary>
        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
        #endregion
    }

}
