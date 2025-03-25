using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ProductVerificationRequest
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public Guid CorrelationId { get; set; }
    }
}
