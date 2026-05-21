using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using MycafePOS.DTOs.Order;
using MycafePOS.Entities;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string search = "",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Menu)
            .AsQueryable();

        // 1. กรองตาม Search (Order Id)
        if (!string.IsNullOrEmpty(search) && int.TryParse(search, out int orderId))
        {
            query = query.Where(o => o.Id == orderId);
        }

        // 2. กรองตามวันที่
        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(o => o.CreatedAt <= endOfDay);
        }

        // 3. นับจำนวนทั้งหมดก่อนแบ่งหน้า (สำหรับทำ TotalPages)
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // 4. แบ่งหน้า (Pagination) และ Select เข้า DTO
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                id = o.Id,
                totalPrice = o.TotalPrice,
                createdAt = o.CreatedAt,
                items = o.OrderItems.Select(oi => new
                {
                    menuName = oi.Menu.MenuName,
                    qty = oi.Qty,
                    pricePerUnit = oi.PricePerUnit,
                    totalPrice = oi.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return Ok(new
        {
            data = orders,
            currentPage = page,
            totalPages = totalPages,
            totalCount = totalCount
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        decimal totalPrice = 0;

        var order = new Orders
        {
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();

        foreach (var item in dto.Items)
        {

            if (item.Qty <= 0)
            {
                return BadRequest("Qty must be greater than 0");
            }

            var menu = await _context.Menu
                .Include(m => m.MenuRecipes)
                .ThenInclude(r => r.Ingredients)
                .FirstOrDefaultAsync(m => m.Id == item.MenuId);

            if (menu == null)
            {
                return BadRequest($"Menu {item.MenuId} not found");
            }

            var itemTotal = menu.MenuPrice * item.Qty;

            totalPrice += itemTotal;

            var orderItem = new OrderItems
            {
                OrderId = order.Id,
                MenuId = menu.Id,
                Qty = item.Qty,
                PricePerUnit = menu.MenuPrice,
                TotalPrice = itemTotal
            };

            _context.OrderItems.Add(orderItem);

            // deduct stock
            foreach (var recipe in menu.MenuRecipes)
            {
                var stock = await _context.IngredientStock
                    .FirstOrDefaultAsync(s =>
                        s.IngredientsId == recipe.IngredientsId);

                if (stock == null)
                {
                    return BadRequest(
                        $"Stock not found for ingredient {recipe.IngredientsId}"
                    );
                }

                stock.Qty -= recipe.Quantity * item.Qty;

                stock.UpdatedAt = DateTime.UtcNow;
            }
        }

        order.TotalPrice = totalPrice;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Order success",
            orderId = order.Id,
            total = totalPrice
        });
    }
}