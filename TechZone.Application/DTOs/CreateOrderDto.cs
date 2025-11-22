namespace TechZone.Application.DTOs
{
    public class CreateOrderDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public object OrderItems { get; internal set; }
        public string UserId { get; set; }
    }
}
