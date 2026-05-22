using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using MycafePOS.Entities;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientsTypeController : ControllerBase
{
    private readonly AppDbContext _context;

    public IngredientsTypeController(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _context.IngredientsType
            .Where(w => w.IsDeleted != true)
            .Select(t => new
            {
                id = t.Id,
                name = t.IngredientsTypeName
            })
            .ToListAsync();

        return Ok(types);
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string name)
    {
        var type = new IngredientsType
        {
            IngredientsTypeName = name
        };

        _context.IngredientsType.Add(type);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = type.Id,
            name = type.IngredientsTypeName
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] string name)
    {
        var type = await _context.IngredientsType
            .FirstOrDefaultAsync(x => x.Id == id);

        if (type == null)
        {
            return NotFound();
        }

        type.IngredientsTypeName = name;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = type.Id,
            name = type.IngredientsTypeName
        });
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _context.IngredientsType
            .FirstOrDefaultAsync(x => x.Id == id);

        if (type == null)
        {
            return NotFound();
        }

        type.IsDeleted = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Deleted"
        });
    }
}