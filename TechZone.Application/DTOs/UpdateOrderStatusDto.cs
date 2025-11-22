using TechZone.Domain.Enums;

namespace TechZone.Application.DTOs
{
    public class UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
    }
}
