using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Bastion.Models;

public class AuthUserInputModel : PageModel
{
    [Required]
    [StringLength(5000, ErrorMessage = "Message is too long")]
    public string SecretPlaintext { get; set; }

    [Required]
    [Range(1, 24)]
    public int Lifetime { get; set; } = 1;

    [Required]
    public string[] OIDReceiver { get; set; } = {"0"};
}
