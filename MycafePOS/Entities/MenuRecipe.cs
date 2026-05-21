using System.ComponentModel.DataAnnotations.Schema;

namespace MycafePOS.Entities;

public class MenuRecipe
{
    public int Id { get; set; }

    public int MenuId { get; set; }

    public Menu? Menu { get; set; }

    public int IngredientsId { get; set; }

    public Ingredients? Ingredients { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Quantity { get; set; }
}