using CustomerService.Validator;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerServiceTest.Validator
{
    public class CustomerValidationConsumerTest
    {
        [Fact]
        public void Consumer_Constructor_InitializesSuccessfully()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CustomerValidationConsumer>>();

            // Act
            var consumer = new CustomerValidationConsumer(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(consumer);
            consumer.Dispose();
        }

        [Fact]
        public async Task ExecuteAsync_StartsConsumerSuccessfully()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<CustomerValidationConsumer>>();

            var consumer = new CustomerValidationConsumer(mockServiceProvider.Object);

            // Act
            var task = consumer.StartAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(task);

            // Clean up
            consumer.Dispose();
        }
    }
}
