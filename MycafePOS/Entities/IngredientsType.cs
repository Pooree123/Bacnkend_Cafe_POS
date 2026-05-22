using System.ComponentModel.DataAnnotations;

namespace MycafePOS.Entities;

public class IngredientsType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string IngredientsTypeName { get; set; } = string.Empty;

    public ICollection<Ingredients>? Ingredients { get; set; }

    public bool IsDeleted { get; set; } = false;
}