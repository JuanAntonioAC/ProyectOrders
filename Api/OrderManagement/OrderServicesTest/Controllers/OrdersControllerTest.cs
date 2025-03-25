using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderServices.Controllers;
using OrderServices.Data;
using OrderServices.Data.DTOS;
using OrderServices.Data.Models;
using OrderServices.Stocks.Interfaces;
using Shared.Models;
using Shared.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderServicesTest.Controllers
{
    public class OrdersControllerTest
    {
        private readonly Mock<IRepository<Order, OrderDbContext>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IProductStockValidatorSender> _mockProductValidator;
        private readonly Mock<ICustomerValidatorSender> _mockCustomerValidator;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTest()
        {
            _mockRepo = new Mock<IRepository<Order, OrderDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockProductValidator = new Mock<IProductStockValidatorSender>();
            _mockCustomerValidator = new Mock<ICustomerValidatorSender>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(
                _mockRepo.Object,
                _mockMapper.Object,
                _mockProductValidator.Object,
                _mockCustomerValidator.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetOrder_InvalidId_Returns404()
        {
            _controller.ModelState.AddModelError("Id", "Required");
            var result = await _controller.GetOrder(0);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrder_NotFound_Returns404()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order)null);
            var result = await _controller.GetOrder(999);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetOrder_Found_ReturnsOk()
        {
            var order = new Order { Id = 1, OrderDate = System.DateTime.Now, Quantity = 2, TotalAmount = 100 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

            var result = await _controller.GetOrder(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            Assert.Equal(2, (int)value.Quantity);
        }

        [Fact]
        public async Task CreateOrder_Valid_ReturnsOk()
        {
            var dto = new CreateOrderDto { ProductId = 1, Quantity = 2, CustomerId = 1 };
            _mockProductValidator.Setup(v => v.ValidateStockAsync(dto.ProductId, dto.Quantity)).ReturnsAsync(new ProductStockValidationResponse { IsAvailable = true });
            _mockCustomerValidator.Setup(v => v.ValidateCustomerAsync(dto.CustomerId)).ReturnsAsync(new CustomerValidationResponse { Exists = true });
            _mockMapper.Setup(m => m.Map<Order>(It.IsAny<CreateOrderDto>())).Returns(new Order());

            var result = await _controller.CreateOrder(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateOrder_Invalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Order", "Invalid");
            var result = await _controller.CreateOrder(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_NotFound_ReturnsNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order)null);
            var result = await _controller.DeleteOrder(1);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_Valid_ReturnsOk()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Order());
            var result = await _controller.DeleteOrder(1);
            Assert.IsType<OkResult>(result);
        }
    }
}
