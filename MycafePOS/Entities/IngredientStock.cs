using System.ComponentModel.DataAnnotations.Schema;

namespace MycafePOS.Entities;

public class IngredientStock
{
    public int Id { get; set; }

    public int IngredientsId { get; set; }

    public Ingredients? Ingredients { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Qty { get; set; }

    public DateTime UpdatedAt { get; set; }
}