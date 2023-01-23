using System.ComponentModel.DataAnnotations;

namespace Bastion.Pages
{
    public class ExampleModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Too long plaintext")] // TODO: Set this limit
        public string? Plaintext { get; set; }

        [Required]
        public int? Lifetime { get; set; }
    }
}
