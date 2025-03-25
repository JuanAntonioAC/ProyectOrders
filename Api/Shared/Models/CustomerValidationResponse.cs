using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class CustomerValidationResponse
    {
        public Guid CorrelationId { get; set; }
        public bool Exists { get; set; }
        public string? Message { get; set; }
    }
}
