using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Shared.Models;
using System.Text.Json;
using System.Text;
using Shared.Models;
using OrderServices.Stocks.Interfaces;

namespace OrderServices.Stocks
{
    public class ProductStockValidatorSender : IProductStockValidatorSender
    {
        #region Private Properties
        private readonly ConnectionFactory _factory;
        private readonly ILogger _logger;
        #endregion


        #region LifeCycle
        /// <summary>
        /// Constructor ProductStockValidatiorSender
        /// </summary>
        /// <param name="hostName"></param>
        public ProductStockValidatorSender( string hostName = "localhost" )
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                DispatchConsumersAsync = true
            };
            
        }
        #endregion


        #region PublicMethod
        /// <summary>
        /// Validate stock 
        /// </summary>
        /// <param name="productId">Prodcut Id</param>
        /// <param name="quantity"> Quantity</param>
        /// <returns>ProductStockValidationResponse</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<ProductStockValidationResponse> ValidateStockAsync(int productId, int quantity)
        {
            try
            {
                using var connection = _factory.CreateConnection();
                using var channel = connection.CreateModel();

                var correlationId = Guid.NewGuid();
                var responseQueue = $"product_response_{correlationId}";

                channel.QueueDeclare("product_validate", durable: false, exclusive: false, autoDelete: false);
                channel.QueueDeclare(responseQueue, durable: false, exclusive: false, autoDelete: true);

                var tcs = new TaskCompletionSource<ProductStockValidationResponse>();

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var response = JsonSerializer.Deserialize<ProductStockValidationResponse>(responseJson);
                    if (response != null && response.CorrelationId == correlationId)
                    {
                        tcs.TrySetResult(response);
                    }
                };

                channel.BasicConsume(queue: responseQueue, autoAck: true, consumer: consumer);

                var request = new ProductStockValidationRequest
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CorrelationId = correlationId
                };

                var requestBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));
                var props = channel.CreateBasicProperties();
                props.CorrelationId = correlationId.ToString();

                channel.BasicPublish(
                    exchange: "",
                    routingKey: "product_validate",
                    basicProperties: props,
                    body: requestBody);

                await Task.Delay(300);

                var timeoutTask = Task.Delay(8000);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                   
                    throw new TimeoutException("Time out");
                }

                return await tcs.Task;
            }
            catch (Exception ex)
            {

                //_logger.LogError(ex, "ProductStockValidatorSender-ValidateStockAsync");
                return default;
      
            }
        }
        #endregion
    }
}
