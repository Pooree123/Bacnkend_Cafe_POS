using System.ComponentModel.DataAnnotations.Schema;

namespace MycafePOS.Entities;

public class OrderItems
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Orders Order { get; set; } = null!;

    public int MenuId { get; set; }

    public Menu Menu { get; set; } = null!;

    public int Qty { get; set; }

    public decimal PricePerUnit { get; set; }

    public decimal TotalPrice { get; set; }
}