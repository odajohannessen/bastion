using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Bastion.Pages
{
    public class UserInputModel : PageModel
    {
        [Required]
        //[StringLength(1, ErrorMessage = "Too long plaintext")] // TODO: Need an upper limit?
        public string SecretPlaintext { get; set; } = "";

        [Required]
        public int Lifetime { get; set; } = 1;
    }
}
