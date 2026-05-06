using System.ComponentModel.DataAnnotations;

namespace HomeMedicineCabinet.UI.Models;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Имя")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}