using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class CustomerValidationRequest
    {
        public int CustomerId { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
