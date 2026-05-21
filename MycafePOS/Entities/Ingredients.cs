using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MycafePOS.Entities;

public class Ingredients
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string IngredientsName { get; set; } = string.Empty;

    public IngredientUnitType IngredientsUnitType { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal CostPerUnit { get; set; }

    public int IngredientsTypeId { get; set; }

    public IngredientsType? IngredientsType { get; set; }

    public IngredientStock? IngredientStock { get; set; }

    public ICollection<MenuRecipe>? MenuRecipes { get; set; }
}