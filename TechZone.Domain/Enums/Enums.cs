namespace TechZone.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        PayPal = 3,
        Stripe = 4,
        CashOnDelivery = 5
    }

    public enum ProductStatus
    {
        Active = 1,
        Inactive = 2,
        OutOfStock = 3,
        Discontinued = 4
    }

    public enum UserRole
    {
        Customer = 1,
        Admin = 2,
        SuperAdmin = 3
    }
}