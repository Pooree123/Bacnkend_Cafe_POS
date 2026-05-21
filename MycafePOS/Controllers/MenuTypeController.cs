using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using MycafePOS.Entities;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuTypeController : ControllerBase
{
    private readonly AppDbContext _context;

    public MenuTypeController(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _context.MenuType
            .Select(t => new
            {
                id = t.Id,
                name = t.MenuTypeName
            })
            .ToListAsync();

        return Ok(types);
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] string name)
    {
        var type = new MenuType
        {
            MenuTypeName = name
        };

        _context.MenuType.Add(type);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = type.Id,
            name = type.MenuTypeName
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] string name)
    {
        var type = await _context.MenuType
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type == null)
        {
            return NotFound(new
            {
                message = "Menu type not found"
            });
        }

        type.MenuTypeName = name;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = type.Id,
            name = type.MenuTypeName
        });
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _context.MenuType
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type == null)
        {
            return NotFound(new
            {
                message = "Menu type not found"
            });
        }

        // check menu using this type
        var used = await _context.Menu
            .AnyAsync(m => m.MenuTypeId == id);

        if (used)
        {
            return BadRequest(new
            {
                message = "This type is being used by menu"
            });
        }

        _context.MenuType.Remove(type);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Delete success"
        });
    }
}