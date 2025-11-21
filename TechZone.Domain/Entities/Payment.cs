using TechZone.Domain.Common;
using TechZone.Domain.Enums;

namespace TechZone.Domain.Entities
{
    /// <summary>
    /// Payment Entity
    /// </summary>
    public class Payment : BaseEntity
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? TransactionId { get; set; }
        public string? PaymentGatewayResponse { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public Order Order { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}