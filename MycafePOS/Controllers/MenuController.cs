using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using MycafePOS.DTOs.Menu;
using MycafePOS.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _context;

    public MenuController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE MENU
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateMenuDto dto)
    {
        decimal totalCost = 0;

        foreach (var recipe in dto.Recipes)
        {
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Id == recipe.IngredientsId);

            if (ingredient == null)
            {
                return BadRequest($"Ingredient {recipe.IngredientsId} not found");
            }

            totalCost += recipe.Quantity * ingredient.CostPerUnit;
        }

        string? imagePath = null;

        if (dto.MenuImg != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.MenuImg.FileName)}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.MenuImg.CopyToAsync(stream);
            }

            imagePath = $"/uploads/menu/{fileName}";
        }

        var menu = new Menu
        {
            MenuName = dto.MenuName,
            MenuImg = imagePath,
            MenuPrice = dto.MenuPrice,
            MenuCost = totalCost,
            Vat = dto.Vat,
            MenuTypeId = dto.MenuTypeId,
            IsDeleted = false // กำหนดค่าเริ่มต้นเป็น false
        };

        _context.Menu.Add(menu);
        await _context.SaveChangesAsync();

        // save recipe
        foreach (var recipe in dto.Recipes)
        {
            var menuRecipe = new MenuRecipe
            {
                MenuId = menu.Id,
                IngredientsId = recipe.IngredientsId,
                Quantity = recipe.Quantity
            };

            _context.MenuRecipe.Add(menuRecipe);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = menu.Id,
            name = menu.MenuName,
            price = menu.MenuPrice,
            cost = menu.MenuCost,
            profit = menu.MenuPrice - menu.MenuCost
        });
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll(string? search, int page = 1, int pageSize = 10)
    {
        var query = _context.Menu
            .Include(m => m.MenuType)
            .Where(m => !m.IsDeleted) // กรองเอาเฉพาะเมนูที่ยังไม่ถูกลบ (Soft Delete)
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.MenuName.ToLower().Contains(search.ToLower()));
        }

        // TOTAL COUNT
        var totalItems = await query.CountAsync();

        // PAGINATION
        var menus = await query
            .OrderByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.MenuName,
                m.MenuImg,
                m.MenuCost,
                m.MenuPrice,
                m.Vat,
                type = m.MenuType.MenuTypeName
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            data = menus
        });
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var menu = await _context.Menu
            .Include(m => m.MenuType)
            .Include(m => m.MenuRecipes)
                .ThenInclude(r => r.Ingredients)
            // เช็คว่าต้องเป็นเมนูที่ยังไม่ถูกลบด้วย
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (menu == null)
        {
            return NotFound(new { message = "Menu not found" });
        }

        return Ok(new
        {
            id = menu.Id,
            name = menu.MenuName,
            image = menu.MenuImg,
            price = menu.MenuPrice,
            cost = menu.MenuCost,
            vat = menu.Vat,
            profit = menu.MenuPrice - menu.MenuCost,
            type = new
            {
                id = menu.MenuType.Id,
                name = menu.MenuType.MenuTypeName
            },
            recipes = menu.MenuRecipes.Select(r => new
            {
                ingredientId = r.IngredientsId,
                ingredientName = r.Ingredients.IngredientsName,
                quantity = r.Quantity,
                unit = r.Ingredients.IngredientsUnitType.ToString(),
                costPerUnit = r.Ingredients.CostPerUnit,
                totalCost = r.Quantity * r.Ingredients.CostPerUnit
            })
        });
    }

    // UPDATE MENU
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] CreateMenuDto dto)
    {
        var menu = await _context.Menu
            .Include(m => m.MenuRecipes)
            // ป้องกันไม่ให้แก้ไขเมนูที่ถูกลบไปแล้ว
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (menu == null)
        {
            return NotFound(new { message = "Menu not found" });
        }

        decimal totalCost = 0;

        // calculate new cost
        foreach (var recipe in dto.Recipes)
        {
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Id == recipe.IngredientsId);

            if (ingredient == null)
            {
                return BadRequest($"Ingredient {recipe.IngredientsId} not found");
            }

            totalCost += recipe.Quantity * ingredient.CostPerUnit;
        }

        string? imagePath = menu.MenuImg;

        if (dto.MenuImg != null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.MenuImg.FileName)}";
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.MenuImg.CopyToAsync(stream);
            }

            imagePath = $"/uploads/menu/{fileName}";
        }

        // update menu
        menu.MenuName = dto.MenuName;
        menu.MenuImg = imagePath;
        menu.MenuPrice = dto.MenuPrice;
        menu.MenuCost = totalCost;
        menu.Vat = dto.Vat;
        menu.MenuTypeId = dto.MenuTypeId;

        // remove old recipe
        _context.MenuRecipe.RemoveRange(menu.MenuRecipes);

        // add new recipe
        foreach (var recipe in dto.Recipes)
        {
            var menuRecipe = new MenuRecipe
            {
                MenuId = menu.Id,
                IngredientsId = recipe.IngredientsId,
                Quantity = recipe.Quantity
            };

            _context.MenuRecipe.Add(menuRecipe);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Update success" });
    }

    // DELETE MENU
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var menu = await _context.Menu
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (menu == null)
        {
            return NotFound(new { message = "Menu not found" });
        }

        menu.IsDeleted = true; // อัปเดตสถานะเป็นซ่อน

        await _context.SaveChangesAsync();

        return Ok(new { message = "Delete success (Soft Delete)" });
    }
}