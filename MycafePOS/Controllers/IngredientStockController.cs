using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using MycafePOS.DTOs.Ingredients;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientStockController : ControllerBase
{
    private readonly AppDbContext _context;

    public IngredientStockController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPut("{ingredientId}")]
    public async Task<IActionResult> UpdateStock(
        int ingredientId,
        UpdateStockDto dto)
    {
        var stock = await _context.IngredientStock
            .FirstOrDefaultAsync(
                s => s.IngredientsId == ingredientId
            );

        if (stock == null)
        {
            return NotFound();
        }

        stock.Qty = dto.Qty;

        stock.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(stock);
    }
}