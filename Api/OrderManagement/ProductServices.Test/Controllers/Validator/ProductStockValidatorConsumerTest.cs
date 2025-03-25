using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProductServices.Data.Models;
using ProductServices.Data;
using ProductServices.Validator;
;

namespace ProductServices.Test.Controllers.Validator
{
    public class ProductStockValidatorConsumerTest
    {
        [Fact]
        public void Consumer_Constructor_InitializesConnectionAndChannel()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var consumer = new ProductStockValidatorConsumer(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(consumer);
            consumer.Dispose();
        }

        [Fact]
        public async Task ExecuteAsync_RunsWithoutExceptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            var consumer = new ProductStockValidatorConsumer(serviceProviderMock.Object);

            // Act
            var result = consumer.StartAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            // Clean
            consumer.Dispose();
        }
    }
}
