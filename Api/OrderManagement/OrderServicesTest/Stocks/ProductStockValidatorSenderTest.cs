using OrderServices.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServicesTest.Stocks
{
    public class ProductStockValidatorSenderTest
    {
        [Fact]
        public async Task ValidateStockAsync_ReturnsNull_OnException()
        {
            // Arrange
            var sender = new ProductStockValidatorSender("invalid-host");

            // Act
            var result = await sender.ValidateStockAsync(1, 10);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            // Act
            var sender = new ProductStockValidatorSender("localhost");

            // Assert
            Assert.NotNull(sender);
        }
    }
}
