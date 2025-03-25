using Shared.Models;

namespace OrderServices.Stocks.Interfaces
{
 
        public interface ICustomerValidatorSender
        {
            Task<CustomerValidationResponse> ValidateCustomerAsync(int customerId);
        }

}
