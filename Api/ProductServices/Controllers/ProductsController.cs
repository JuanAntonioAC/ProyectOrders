using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductServices.Data;
using ProductServices.Data.Models;
using Shared.Repositories;
using Shared.Repositories.Interfaces;

namespace ProductServices.Controllers
{
    [Route("api/Products")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        #region Private Methods
        private readonly IRepository<Product, ProductDbContext> _repository;
        private readonly ILogger _logger;
        #endregion

        #region LifeCycle
        public ProductsController(IRepository<Product, ProductDbContext> repository, ILogger<ProductsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        #endregion

        #region Public Method
        [HttpGet("{Id:int}", Name = "GetProduct")]
        public async Task<IActionResult> GetProduct(int Id)
        {
            if (!ModelState.IsValid || Id == 0)
            {
                return StatusCode(404, new { Error = "Invalid Id", TypeError = "Model Error" });
            }
            else
            {
                var product = await _repository.GetByIdAsync(Id);
                if (product == null)
                {
                    return StatusCode(404, new { Error = "Product not found", TypeError = "Model Error" });
                }
                else
                {
                    return Ok(new { product.Name, product.Price });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var list =await  _repository.GetAllAsync();
            if (list == null)
            {
                return NoContent();
            }
            else
            {
                return Ok(list.Select(c => new { c.Name, c.Price, c.Stock }));
            }
        }
        #endregion
    }
}
