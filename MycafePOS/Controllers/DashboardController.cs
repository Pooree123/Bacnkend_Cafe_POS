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

    // =========================
    // DYNAMIC DASHBOARD SUMMARY
    // =========================
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary(
        [FromQuery] string filter = "all",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        // 1. สร้าง Base Query
        var ordersQuery = _context.Orders.AsQueryable();
        var itemsQuery = _context.OrderItems.Include(oi => oi.Menu).AsQueryable();

        // 2. จัดการเงื่อนไขตาม Dropdown (Filter)
        DateTime now = DateTime.UtcNow;

        if (filter == "month")
        {
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startOfMonth);
            itemsQuery = itemsQuery.Where(oi => oi.Order.CreatedAt >= startOfMonth); // แก้ Order เป็น Orders
        }
        else if (filter == "year")
        {
            var startOfYear = new DateTime(now.Year, 1, 1);
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startOfYear);
            itemsQuery = itemsQuery.Where(oi => oi.Order.CreatedAt >= startOfYear); // แก้ Order เป็น Orders
        }
        else if (filter == "range" && startDate.HasValue && endDate.HasValue)
        {
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value && o.CreatedAt <= endOfDay);
            itemsQuery = itemsQuery.Where(oi => oi.Order.CreatedAt >= startDate.Value && oi.Order.CreatedAt <= endOfDay); // แก้ Order เป็น Orders
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
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month) // เรียงลำดับจากปีและเดือนก่อน Select
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
                .OrderBy(g => g.Key) // แก้ไข: เรียงลำดับวันที่ก่อนทำ Select
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

        var totalCost = await itemsQuery.SumAsync(oi => oi.Qty * oi.Menu.MenuCost);

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
            costProfitData // ส่งข้อมูลก้อนนี้ไปให้กราฟวงกลม
        });
    }

    // =========================
    // LOW STOCK REPORT 
    // =========================
    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock()
    {
        var data = await _context.IngredientStock
            .Include(s => s.Ingredients)
            .Where(s => s.Qty < 10)
            .Select(s => new
            {
                ingredient = s.Ingredients.IngredientsName,
                qty = s.Qty
            })
            .ToListAsync();

        return Ok(data);
    }
}