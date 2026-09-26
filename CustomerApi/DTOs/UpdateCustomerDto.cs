using System.ComponentModel.DataAnnotations;

namespace CustomerApi.DTOs
{
    public class UpdateCustomerDto
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}
