using System.ComponentModel.DataAnnotations;

namespace OrderServices.Data.DTOS
{
    public record CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int ProductId { get; set; }


        [Required]
        public int Quantity { get; set; }

        

    }
}
