using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using MycafePOS.DTOs.Ingredients;
using MycafePOS.Entities;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientsController : ControllerBase
{
    private readonly AppDbContext _context;

    public IngredientsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
     [FromQuery] string? search,
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10)
    {
        // ✨ แก้ไขจุดที่ 1: กรองเอาตัวที่ยังไม่โดนลบตั้งแต่ต้นทาง query เลย
        var query = _context.Ingredients
            .Include(i => i.IngredientsType)
            .Include(i => i.IngredientStock)
            .Where(i => !i.IsDeleted)
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.IngredientsName.ToLower()
                    .Contains(search.ToLower()));
        }

        // ตอนนี้ค่า Count จะถูกต้องตามความเป็นจริงแล้ว ไม่รวมตัวที่โดน Soft Delete
        var totalItems = await query.CountAsync();

        var ingredients = await query
            .OrderByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            data = ingredients.Select(i => new
            {
                id = i.Id,
                name = i.IngredientsName,
                unit = i.IngredientsUnitType,
                costPerUnit = i.CostPerUnit,
                type = i.IngredientsType != null
                    ? i.IngredientsType.IngredientsTypeName
                    : null,
                stock = i.IngredientStock != null
                    ? i.IngredientStock.Qty
                    : 0
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngredientsDto dto)
    {
        var ingredient = new Ingredients
        {
            IngredientsName = dto.IngredientsName,
            IngredientsUnitType = dto.IngredientsUnitType,
            CostPerUnit = dto.CostPerUnit,
            IngredientsTypeId = dto.IngredientsTypeId
        };

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        // create stock default
        var stock = new IngredientStock
        {
            IngredientsId = ingredient.Id,
            Qty = 0,
            UpdatedAt = DateTime.UtcNow
        };

        _context.IngredientStock.Add(stock);
        await _context.SaveChangesAsync();

        // load relation
        await _context.Entry(ingredient).Reference(i => i.IngredientsType).LoadAsync();
        await _context.Entry(ingredient).Reference(i => i.IngredientStock).LoadAsync();

        // ✨ แก้ไขจุดที่ 2: แนบ message และห่อ Object ข้อมูลกลับไปให้หน้าบ้านอ่านง่ายๆ
        return Ok(new
        {
            message = "Create Success",
            data = new
            {
                id = ingredient.Id,
                name = ingredient.IngredientsName,
                unit = ingredient.IngredientsUnitType,
                costPerUnit = ingredient.CostPerUnit,
                type = ingredient.IngredientsType?.IngredientsTypeName,
                stock = ingredient.IngredientStock?.Qty ?? 0
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIngredientsDto dto)
    {
        var ingredient = await _context.Ingredients
            .Include(i => i.IngredientsType)
            .Include(i => i.IngredientStock)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient == null)
        {
            return NotFound(new { message = "Ingredient not found" });
        }

        ingredient.IngredientsName = dto.IngredientsName;
        ingredient.IngredientsUnitType = dto.IngredientsUnitType;
        ingredient.CostPerUnit = dto.CostPerUnit;
        ingredient.IngredientsTypeId = dto.IngredientsTypeId;

        await _context.SaveChangesAsync();

        // ✨ แก้ไขจุดที่ 3: แนบ message กลับไปเหมือนกับตอนสัญญากับหน้าบ้านไว้
        return Ok(new
        {
            message = "Update Success",
            data = new
            {
                id = ingredient.Id,
                name = ingredient.IngredientsName,
                unit = ingredient.IngredientsUnitType,
                costPerUnit = ingredient.CostPerUnit,
                type = ingredient.IngredientsType?.IngredientsTypeName,
                stock = ingredient.IngredientStock?.Qty ?? 0
            }
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient == null)
        {
            return NotFound(new { message = "Ingredient not found" });
        }

        ingredient.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Soft Deleted Success"
        });
    }
}