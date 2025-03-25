using CustomerService.Data;
using CustomerService.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Repositories;
using Shared.Repositories.Interfaces;


namespace CustomerService.Controllers
{
    [Route("api/Costumer")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        #region Private Properties
        private readonly IRepository<Customer , CustomerDbContext> _repository;
        private readonly ILogger _logger;
        #endregion

        #region LifeCycle
        /// <summary>
        /// Constructor, CustomerController
        /// </summary>
        /// <param name="repository"></param>
        public CustomersController(IRepository<Customer, CustomerDbContext> repository, ILogger<CustomersController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get Customer by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id:int}", Name = "GetCustomerDetails")]
        public async Task<IActionResult> GetCustomerDetails(int Id)
        {
            try
            {
                if (!ModelState.IsValid || Id == 0)
                {
                    return StatusCode(404, new { Error = "Invalid Id", TypeError = "Model Error" });
                }
                else
                {
                    var customer = await _repository.GetByIdAsync(Id);
                    if (customer == null)
                    {
                        return StatusCode(404, new { Error = "Customer not found", TypeError = "Model Error" });
                    }
                    else
                    {
                        return Ok(new { Name = $"{customer.FirstName} {customer.LastName}", Email = customer.Email });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetCustomerDetails");
                return StatusCode(500, ex);
            }
        }
        #endregion
    }
}
