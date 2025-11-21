using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;
using TechZone.Domain.Enums;
using TechZone.Domain.Interfaces;

namespace TechZone.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // Validate order items
            if (dto.OrderItems == null || !dto.OrderItems.Any())
                throw new Exception("Order must contain at least one item");

            // Generate unique order number
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

            // Calculate totals
            decimal subTotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.OrderItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {item.ProductId} not found");

                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"Insufficient stock for product: {product.Name}");

                var unitPrice = product.DiscountPrice ?? product.Price;
                var totalPrice = unitPrice * item.Quantity;
                subTotal += totalPrice;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });

                // Reduce stock quantity
                product.StockQuantity -= item.Quantity;
                await _unitOfWork.Repository<Product>().UpdateAsync(product);
            }

            // Calculate additional costs
            var shippingCost = 50m; // Fixed shipping cost
            var tax = subTotal * 0.18m; // 18% tax
            var totalAmount = subTotal + shippingCost + tax - dto.OrderItems.Sum(x => 0); // discount applied here

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = dto.UserId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                SubTotal = subTotal,
                ShippingCost = shippingCost,
                Tax = tax,
                DiscountAmount = 0,
                TotalAmount = totalAmount,
                ShippingFirstName = dto.ShippingFirstName,
                ShippingLastName = dto.ShippingLastName,
                ShippingEmail = dto.ShippingEmail,
                ShippingPhone = dto.ShippingPhone,
                ShippingAddress = dto.ShippingAddress,
                ShippingCity = dto.ShippingCity,
                ShippingState = dto.ShippingState,
                ShippingZipCode = dto.ShippingZipCode,
                ShippingCountry = dto.ShippingCountry,
                BillingFirstName = dto.BillingFirstName,
                BillingLastName = dto.BillingLastName,
                BillingAddress = dto.BillingAddress,
                BillingCity = dto.BillingCity,
                BillingState = dto.BillingState,
                BillingZipCode = dto.BillingZipCode,
                BillingCountry = dto.BillingCountry,
                Notes = dto.Notes
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Add order items
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                await _unitOfWork.Repository<OrderItem>().AddAsync(item);
            }

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = dto.UserId,
                Amount = totalAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = PaymentStatus.Pending
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id) ?? new OrderDto();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order == null) return null;

            var orderItems = await _unitOfWork.Repository<OrderItem>()
                .FindAsync(oi => oi.OrderId == id);

            var payments = await _unitOfWork.Repository<Payment>()
                .FindAsync(p => p.OrderId == id);
            var payment = payments.FirstOrDefault();

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                SubTotal = order.SubTotal,
                ShippingCost = order.ShippingCost,
                Tax = order.Tax,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                ShippingFirstName = order.ShippingFirstName,
                ShippingLastName = order.ShippingLastName,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingZipCode = order.ShippingZipCode,
                ShippingCountry = order.ShippingCountry,
                TrackingNumber = order.TrackingNumber,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                OrderItems = orderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList(),
                Payment = payment != null ? new PaymentDto
                {
                    Id = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    Status = payment.Status,
                    TransactionId = payment.TransactionId,
                    PaidDate = payment.PaidDate
                } : null
            };
        }

        public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _unitOfWork.Repository<Order>()
                .FindAsync(o => o.UserId == userId);

            var orderDtos = new List<OrderDto>();
            foreach (var order in orders.OrderByDescending(o => o.OrderDate))
            {
                var orderDto = await GetOrderByIdAsync(order.Id);
                if (orderDto != null)
                    orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public async Task<PagedResponse<OrderDto>> GetAllOrdersAsync(int pageNumber, int pageSize)
        {
            var allOrders = await _unitOfWork.Repository<Order>().GetAllAsync();
            var ordersQuery = allOrders.OrderByDescending(o => o.OrderDate);

            var totalRecords = ordersQuery.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var orders = ordersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var orderDtos = new List<OrderDto>();
            foreach (var order in orders)
            {
                var orderDto = await GetOrderByIdAsync(order.Id);
                if (orderDto != null)
                    orderDtos.Add(orderDto);
            }

            return new PagedResponse<OrderDto>
            {
                Items = orderDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(dto.OrderId);
            if (order == null) return false;

            order.Status = dto.Status;
            order.TrackingNumber = dto.TrackingNumber;
            order.Notes = dto.Notes;

            if (dto.Status == OrderStatus.Shipped && !order.ShippedDate.HasValue)
            {
                order.ShippedDate = DateTime.UtcNow;
            }
            else if (dto.Status == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
            {
                order.DeliveredDate = DateTime.UtcNow;
            }

            await _unitOfWork.Repository<Order>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null) return false;

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
                throw new Exception("Cannot cancel this order");

            // Restore product stock
            var orderItems = await _unitOfWork.Repository<OrderItem>()
                .FindAsync(oi => oi.OrderId == orderId);

            foreach (var item in orderItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    await _unitOfWork.Repository<Product>().UpdateAsync(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _unitOfWork.Repository<Order>().UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}