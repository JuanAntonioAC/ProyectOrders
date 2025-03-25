using System.ComponentModel.DataAnnotations;

namespace CustomerService.Data.DTOs
{
    public class CustomerDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }

}
