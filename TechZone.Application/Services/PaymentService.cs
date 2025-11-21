using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;
using TechZone.Domain.Enums;
using TechZone.Domain.Interfaces;

namespace TechZone.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentDto> ProcessPaymentAsync(int orderId, PaymentMethod method)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found");

            var payments = await _unitOfWork.Repository<Payment>()
                .FindAsync(p => p.OrderId == orderId);
            var payment = payments.FirstOrDefault();

            if (payment == null)
            {
                payment = new Payment
                {
                    OrderId = orderId,
                    UserId = order.UserId,
                    Amount = order.TotalAmount,
                    PaymentMethod = method,
                    Status = PaymentStatus.Pending
                };
                await _unitOfWork.Repository<Payment>().AddAsync(payment);
            }

            // Simulate payment processing
            // In production, integrate with actual payment gateway (Stripe, PayPal, etc.)
            bool paymentSuccess = await SimulatePaymentGateway(method, order.TotalAmount);

            if (paymentSuccess)
            {
                payment.Status = PaymentStatus.Completed;
                payment.TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
                payment.PaidDate = DateTime.UtcNow;
                payment.PaymentGatewayResponse = "Payment successful";

                // Update order status
                order.Status = OrderStatus.Confirmed;
                await _unitOfWork.Repository<Order>().UpdateAsync(order);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.PaymentGatewayResponse = "Payment failed";
            }

            await _unitOfWork.Repository<Payment>().UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                PaidDate = payment.PaidDate
            };
        }

        public async Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId)
        {
            var payments = await _unitOfWork.Repository<Payment>()
                .FindAsync(p => p.OrderId == orderId);
            var payment = payments.FirstOrDefault();

            if (payment == null) return null;

            return new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                PaidDate = payment.PaidDate
            };
        }

        public async Task<bool> RefundPaymentAsync(int paymentId)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);
            if (payment == null) return false;

            if (payment.Status != PaymentStatus.Completed)
                throw new Exception("Can only refund completed payments");

            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(payment.OrderId);
            if (order == null) return false;

            // Simulate refund processing
            bool refundSuccess = await SimulateRefundGateway(payment.TransactionId);

            if (refundSuccess)
            {
                payment.Status = PaymentStatus.Refunded;
                payment.PaymentGatewayResponse = "Refund successful";

                // Update order status
                order.Status = OrderStatus.Refunded;
                await _unitOfWork.Repository<Order>().UpdateAsync(order);

                // Restore product stock
                var orderItems = await _unitOfWork.Repository<OrderItem>()
                    .FindAsync(oi => oi.OrderId == order.Id);

                foreach (var item in orderItems)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        await _unitOfWork.Repository<Product>().UpdateAsync(product);
                    }
                }

                await _unitOfWork.Repository<Payment>().UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // Mock payment gateway simulation
        private Task<bool> SimulatePaymentGateway(PaymentMethod method, decimal amount)
        {
            // Simulate payment processing delay
            Task.Delay(1000).Wait();

            // In production, integrate with real payment gateway
            // For demo, 95% success rate
            return Task.FromResult(new Random().Next(100) < 95);
        }

        // Mock refund gateway simulation
        private Task<bool> SimulateRefundGateway(string? transactionId)
        {
            // Simulate refund processing delay
            Task.Delay(1000).Wait();

            // In production, integrate with real payment gateway
            return Task.FromResult(true);
        }
    }
}