using Shared.Models;

namespace OrderServices.Stocks.Interfaces
{
    public interface IProductStockValidatorSender
    {
        Task<ProductStockValidationResponse> ValidateStockAsync(int productId, int quantity);
    }
}
