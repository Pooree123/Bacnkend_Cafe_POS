using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MycafePOS.Entities;

public class Menu
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string MenuName { get; set; } = string.Empty;

    public string Menudescription { get; set; } = string.Empty;

    public string? MenuImg { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MenuCost { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MenuPrice { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal Vat { get; set; }

    public int MenuTypeId { get; set; }

    public MenuType? MenuType { get; set; }

    public ICollection<MenuRecipe>? MenuRecipes { get; set; }

    public ICollection<OrderItems>? OrderItems { get; set; }

    // ====== เพิ่ม Property สำหรับ Soft Delete ======
    public bool IsDeleted { get; set; } = false;
}