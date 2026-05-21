using System.ComponentModel.DataAnnotations.Schema;

namespace MycafePOS.Entities;

public class Orders
{
    public int Id { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItems> OrderItems { get; set; }
        = new List<OrderItems>();
}