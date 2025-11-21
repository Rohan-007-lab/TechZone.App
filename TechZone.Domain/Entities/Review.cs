using TechZone.Domain.Common;

namespace TechZone.Domain.Entities
{
    /// <summary>
    /// Product Review Entity
    /// </summary>
    public class Review : BaseEntity
    {
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsApproved { get; set; } = false;
        public int HelpfulCount { get; set; } = 0;

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}