using CustomerService.Controllers;
using CustomerService.Data;
using CustomerService.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

using Shared.Repositories.Interfaces;


namespace CustomerServiceTest.Controllers
{
    public class CustomersControllerTest
    {
        private readonly Mock<IRepository<Customer, CustomerDbContext>> _mockRepo;
        private readonly Mock<ILogger<CustomersController>> _mockLogger;
        private readonly CustomersController _controller;

        public CustomersControllerTest()
        {
            _mockRepo = new Mock<IRepository<Customer, CustomerDbContext>>();
            _mockLogger = new Mock<ILogger<CustomersController>>();
            _controller = new CustomersController(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCustomerDetails_InvalidModel_Returns404()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Required");

            // Act
            var result = await _controller.GetCustomerDetails(0);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetCustomerDetails_NotFound_Returns404()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Customer)null);

            // Act
            var result = await _controller.GetCustomerDetails(999);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetCustomerDetails_ValidId_ReturnsOk()
        {
            // Arrange
            var customer = new Customer { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

            // Act
            var result = await _controller.GetCustomerDetails(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = okResult.Value;
            Assert.Equal("John Doe", (string)data.Name);
            Assert.Equal("john@example.com", (string)data.Email);
        }

        [Fact]
        public async Task GetCustomerDetails_ExceptionThrown_Returns500()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ThrowsAsync(new System.Exception("DB Error"));

            // Act
            var result = await _controller.GetCustomerDetails(1);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}
