using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ProductStockValidationResponse
    {
        public Guid CorrelationId { get; set; }
        public bool IsAvailable { get; set; }
        public string? Message { get; set; }
    }
}
