
namespace TechZone.Application.DTOs
{
    public class CreateOrderDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
       // public object OrderItems { get; internal set; }
        public string UserId { get; set; }

        public string ShippingFirstName { get; set; } = string.Empty;
        public string ShippingLastName { get; set; } = string.Empty;
        public string ShippingEmail { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;

        public string BillingFirstName { get; set; } = string.Empty;
        public string BillingLastName { get; set; } = string.Empty;
        public List<CreateOrderDto> OrderItems { get; set; }
    }
}
