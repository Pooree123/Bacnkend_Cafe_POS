namespace MycafePOS.DTOs.Menu;

public class CreateMenuDto
{
    public string MenuName { get; set; } = string.Empty;

    public IFormFile? MenuImg { get; set; }

    public decimal MenuPrice { get; set; }

    public decimal Vat { get; set; }

    public int MenuTypeId { get; set; }

    public List<MenuRecipeItemDto> Recipes { get; set; }
        = new();
}