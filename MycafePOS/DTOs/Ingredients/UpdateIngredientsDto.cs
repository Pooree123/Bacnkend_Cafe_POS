using MycafePOS.Entities;

namespace MycafePOS.DTOs.Ingredients;

public class UpdateIngredientsDto
{
    public string IngredientsName { get; set; } = string.Empty;

    public IngredientUnitType IngredientsUnitType { get; set; }

    public decimal CostPerUnit { get; set; }

    public int IngredientsTypeId { get; set; }
}