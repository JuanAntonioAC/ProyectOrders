using System.ComponentModel.DataAnnotations;

namespace OrderServices.Data.DTOS
{
    public class UpdateOrderDto
    {

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El valor no puede ser 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El valor no puede ser 0")]
        public decimal TotalAmount { get; set; }
    }
}
