using System.ComponentModel.DataAnnotations;

namespace MycafePOS.Entities;

public class Users
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string HashPassword { get; set; } = string.Empty;
}