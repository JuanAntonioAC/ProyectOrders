using OrderServices.Stocks.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Shared.Models;
using System.Text.Json;
using System.Text;

namespace OrderServices.Stocks
{
    public class CustomerValidatorSender : ICustomerValidatorSender
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
        public CustomerValidatorSender(string hostName = "localhost" )
        {
            _factory = new ConnectionFactory { HostName = hostName, DispatchConsumersAsync = true };
            
        }
        #endregion

        #region PublicMethod
        /// <summary>
        /// Validate Customer
        /// </summary>
        /// <param name="customerId">IdCustomer</param>
        /// <returns>CustomerValidationResponse</returns>
        /// <exception cref="TimeoutException"></exception>
        public async Task<CustomerValidationResponse> ValidateCustomerAsync(int customerId)
        {
            try
            {
                using var connection = _factory.CreateConnection();
                using var channel = connection.CreateModel();

                var correlationId = Guid.NewGuid();
                var responseQueue = $"customer_response_{correlationId}";

                channel.QueueDeclare("customer_validate", durable: false, exclusive: false, autoDelete: false);
                channel.QueueDeclare(responseQueue, durable: false, exclusive: false, autoDelete: true);

                var tcs = new TaskCompletionSource<CustomerValidationResponse>();

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var responseJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var response = JsonSerializer.Deserialize<CustomerValidationResponse>(responseJson);
                    if (response != null && response.CorrelationId == correlationId)
                    {
                        tcs.TrySetResult(response);
                    }
                };

                channel.BasicConsume(queue: responseQueue, autoAck: true, consumer: consumer);

                var request = new CustomerValidationRequest
                {
                    CustomerId = customerId,
                    CorrelationId = correlationId
                };

                var requestBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

                var props = channel.CreateBasicProperties();
                props.CorrelationId = correlationId.ToString();

                channel.BasicPublish(
                    exchange: "",
                    routingKey: "customer_validate",
                    basicProperties: props,
                    body: requestBody);

                await Task.Delay(300);

                var timeoutTask = Task.Delay(10000);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                  
                    throw new TimeoutException("Time out");
                }

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                
                return default;
            }
        }
        #endregion
    }

}
