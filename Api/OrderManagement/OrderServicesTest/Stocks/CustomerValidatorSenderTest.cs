using OrderServices.Stocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServicesTest.Stocks
{
    public class CustomerValidatorSenderTest
    {
        [Fact]
        public async Task ValidateCustomerAsync_ReturnsNull_OnException()
        {
            // Arrange
            var sender = new CustomerValidatorSender("invalid-host");

            // Act
            var result = await sender.ValidateCustomerAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            // Act
            var sender = new CustomerValidatorSender("localhost");

            // Assert
            Assert.NotNull(sender);
        }
    }
}
