using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Bastion.Models;

public class UserInputModel : PageModel
{
    [Required]
    [StringLength(5000, ErrorMessage = "Message is too long")]
    public string SecretPlaintext { get; set; } = "";

    [Required]
    public int Lifetime { get; set; } = 1;
}
