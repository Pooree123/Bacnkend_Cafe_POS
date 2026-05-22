using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MycafePOS.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MycafePOS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary(
    [FromQuery] string filter = "all",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
    {
        var ordersQuery = _context.Orders.AsQueryable();
        var itemsQuery = _context.OrderItems.Include(oi => oi.Menu).AsQueryable();

        DateTime now = DateTime.UtcNow;

        if (filter == "month")
        {
            // 🔥 แก้ไข: ระบุ DateTimeKind.Utc ให้กับวันที่สร้างใหม่
            var startOfMonth = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);

            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startOfMonth);
            itemsQuery = itemsQuery.Where(oi => oi.Order.CreatedAt >= startOfMonth);
        }
        else if (filter == "year")
        {
            // 🔥 แก้ไข: ระบุ DateTimeKind.Utc ให้กับวันที่สร้างใหม่เช่นกัน
            var startOfYear = DateTime.SpecifyKind(new DateTime(now.Year, 1, 1), DateTimeKind.Utc);

            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startOfYear);
            itemsQuery = itemsQuery.Where(oi => oi.Order.CreatedAt >= startOfYear);
        }
        else if (filter == "range" && startDate.HasValue && endDate.HasValue)
        {
            // 🔥 แก้ไข: แปลงค่า startDate และ endDate ที่รับเข้ามาให้เป็น UTC ให้ชัวร์ก่อนนำไปคำนวณต่อ
            var startRange = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            var endOfDay = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startRange && o.CreatedAt <= endOfDay);
            itemsQuery = itemsQuery.Where(oi => oi.Order.CreatedAt >= startRange && oi.Order.CreatedAt <= endOfDay);
        }

        // 3. คำนวณยอดรวม (Total)
        var totalSales = await ordersQuery.SumAsync(o => o.TotalPrice);
        var totalOrders = await ordersQuery.CountAsync();

        // 4. ดึงข้อมูล 3 เมนูที่ขายดีที่สุด (Top 3 Menus)
        var topMenus = await itemsQuery
            .GroupBy(oi => oi.Menu.MenuName)
            .Select(g => new
            {
                menu = g.Key,
                qty = g.Sum(x => x.Qty),
                sales = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.qty)
            .Take(3)
            .ToListAsync();

        // 5. เตรียมข้อมูลกราฟเส้น (Line Chart)
        var rawOrders = await ordersQuery
            .Select(o => new { o.CreatedAt, o.TotalPrice })
            .ToListAsync();

        object lineChartData;

        if (filter == "year" || filter == "all")
        {
            lineChartData = rawOrders
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    label = $"{g.Key.Month:D2}/{g.Key.Year}",
                    sales = g.Sum(x => x.TotalPrice),
                    orders = g.Count()
                })
                .ToList();
        }
        else
        {
            lineChartData = rawOrders
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    label = g.Key.ToString("dd/MM/yyyy"),
                    sales = g.Sum(x => x.TotalPrice),
                    orders = g.Count()
                })
                .ToList();
        }

        var pieChartData = await itemsQuery
            .GroupBy(oi => oi.Menu.MenuName)
            .Select(g => new
            {
                label = g.Key,
                value = g.Sum(x => x.Qty)
            })
            .OrderByDescending(x => x.value)
            .Take(5)
            .ToListAsync();

        var totalCost = await itemsQuery.SumAsync(oi => oi.Qty * (oi.Menu.MenuCost)); // ดักเผื่อ MenuCost เป็น null

        // คำนวณกำไร: เอายอดขายรวม หักลบด้วย ต้นทุนรวม
        var totalProfit = totalSales - totalCost;

        var costProfitData = new[]
        {
        new { label = "ต้นทุน (Cost)", value = totalCost },
        new { label = "กำไร (Profit)", value = totalProfit }
    };

        // ส่งข้อมูลกลับไปให้ Frontend
        return Ok(new
        {
            summary = new { totalSales, totalOrders },
            topMenus,
            lineChartData,
            pieChartData,
            costProfitData
        });
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock(
        int minstock = 50)
    {
        var data = await _context.IngredientStock
            .Include(s => s.Ingredients)
            .Where(s => s.Qty < minstock)
            .Select(s => new
            {
                ingredient = s.Ingredients.IngredientsName,
                qty = s.Qty
            })
            .ToListAsync();

        return Ok(data);
    }
}