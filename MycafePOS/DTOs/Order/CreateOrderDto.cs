namespace MycafePOS.DTOs.Order;

public class CreateOrderDto
{
    public decimal Price { get; set; }
    public List<CreateOrderItemDto> Items { get; set; }
        = new();
}