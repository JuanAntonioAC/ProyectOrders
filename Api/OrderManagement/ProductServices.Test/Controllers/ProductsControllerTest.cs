using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductServices.Controllers;
using ProductServices.Data;
using ProductServices.Data.Models;
using Shared.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductServices.Test.Controllers
{
    public class ProductsControllerTest
    {
        private readonly Mock<IRepository<Product, ProductDbContext>> _mockRepo;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly ProductsController _controller;

        public ProductsControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product, ProductDbContext>>();
            _mockLogger = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetProduct_InvalidId_Returns404()
        {
            // Arrange
            _controller.ModelState.AddModelError("Id", "Required");

            // Act
            var result = await _controller.GetProduct(0);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetProduct_NotFound_Returns404()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product)null);

            // Act
            var result = await _controller.GetProduct(999);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetProduct_Found_ReturnsOk()
        {
            // Arrange
            var product = new Product { Name = "Test", Price = 10.0M };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            Assert.Equal("Test", (string)response.Name);
            Assert.Equal(10.0M, (decimal)response.Price);
        }

        [Fact]
        public async Task GetProducts_ReturnsList()
        {
            // Arrange
            var list = new List<Product>
            {
                new Product { Name = "P1", Price = 10, Stock = 5 },
                new Product { Name = "P2", Price = 20, Stock = 2 },
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetProducts_Empty_ReturnsNoContent()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync((IEnumerable<Product>)null);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
