using System.ComponentModel.DataAnnotations;

namespace RoomManagerApp.Models.Dto;

public class LoginDTO
{
    [Required]
    [EmailAddress]
    public String Email { get; set; } = default!;

    [Required]
    [MinLength(8), MaxLength(40)]
    public String Password { get; set; } = default!;

}
