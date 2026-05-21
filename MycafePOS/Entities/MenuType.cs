using System.ComponentModel.DataAnnotations;

namespace MycafePOS.Entities;

public class MenuType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string MenuTypeName { get; set; } = string.Empty;

    public ICollection<Menu>? Menus { get; set; }
}