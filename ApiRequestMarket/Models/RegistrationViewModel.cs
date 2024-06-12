using Microsoft.AspNetCore.Authorization;

namespace ApiRequestMarket.Models;

using System.ComponentModel.DataAnnotations;

public class RegistrationViewModel
{
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(20, MinimumLength = 6)] 
    public string Password { get; set; }
}
