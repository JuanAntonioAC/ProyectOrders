using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderServices.Data;
using OrderServices.Data.DTOS;
using OrderServices.Data.Models;
using OrderServices.Stocks;
using OrderServices.Stocks.Interfaces;
using Shared.Repositories.Interfaces;

namespace OrderServices.Controllers
{
    [Route("api/Orders")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        #region Private Properties   
        private readonly IRepository<Order , OrderDbContext> _repository;
        private readonly IMapper _mapper;
        private readonly IProductStockValidatorSender _productStockValidatorSender;
        private readonly ICustomerValidatorSender _customerValidatorSender;
        private readonly ILogger _logger;
        #endregion


        #region LifeCycle
        /// <summary>
        /// Constructor OrdersController
        /// </summary>
        /// <param name="repository">Repository</param>
        /// <param name="mapper">Mapper</param>
        /// <param name="productStockValidatorSender">Product Validator</param>
        /// <param name="customerValidatorSender">Customer Validator</param>
        /// <param name="logger">Logger</param>
        public OrdersController(IRepository<Order, OrderDbContext> repository,
                                IMapper mapper, IProductStockValidatorSender productStockValidatorSender, 
                                ICustomerValidatorSender customerValidatorSender, ILogger<OrdersController> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _productStockValidatorSender = productStockValidatorSender;
            _customerValidatorSender = customerValidatorSender;
            _logger = logger;

        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Get All Orders
        /// </summary>
        /// <returns>All Orders</returns>
        [HttpGet(Name = "Get")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var Orders = await _repository.GetAllAsync();
                if (Orders != null)
                {
                    return Ok(Orders.Select(c => new { c.OrderDate, c.Quantity, c.TotalAmount }));
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "Order-GetALL");
                return StatusCode(500, ex);
            }

        }

        /// <summary>
        /// GetOrder by Id
        /// </summary>
        /// <param name="Id">Id order</param>
        /// <returns>Order</returns>
        [HttpGet("{Id:int}", Name = "GetOrder")]
        public async Task<IActionResult> GetOrder(int Id)
        {
            try
            {
                if (!ModelState.IsValid || Id == 0)
                {
                    return StatusCode(404, new { Error = "Invalid Id", TypeError = "Model Error" });
                }
                else
                {
                    var order = await _repository.GetByIdAsync(Id);
                    if (order != null)
                    {
                        return StatusCode(404, new { Error = "Order not found", TypeError = "Model Error" });
                    }
                    else
                    {
                        return Ok(new { order.OrderDate, order.Quantity, order.TotalAmount });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order-GetOrder");
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto order)
        {
            if (!ModelState.IsValid || order == null)
            {
                return BadRequest();
            }
            else
            {
                try
                {
                    var result = await _productStockValidatorSender.ValidateStockAsync(order.ProductId, order.Quantity);
                    var customer = await _customerValidatorSender.ValidateCustomerAsync(order.CustomerId);
                    if (result.IsAvailable && customer.Exists)
                    {
                        var NewOrder = _mapper.Map<Order>(order);
                        await _repository.AddAsync(NewOrder);
                        return Ok(new { NewOrder.Id, NewOrder.OrderDate, NewOrder.Quantity });
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Order-CreateOrder");
                    return StatusCode(500,ex);
                }
             }
            
        }


        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateOrder(int Id,[FromBody] UpdateOrderDto order)
        {
            if (!ModelState.IsValid || order == null || await _repository.GetByIdAsync(Id) != null)
            {
                return BadRequest();
            }
            else
            {
                try
                {
                    var NewOrder = _mapper.Map<Order>(order);
                    NewOrder.Id = Id;
                    await _repository.UpdateAsync(NewOrder);
                    return Ok(order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Order-UpdateOrder");
                    return StatusCode(500, ex);
                }
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> DeleteOrder(int Id)
        {
            try
            {
                var order = await _repository.GetByIdAsync(Id);
                if (order != null)
                {

                    await _repository.DeleteAsync(order);
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order-DeleteOrder");
                return StatusCode(500, ex);
            }
        }
        #endregion

    }
}
