using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Enums;

namespace TechZone.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            var orders = await _orderService.GetAllOrdersAsync(pageNumber, 20);
            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status, string? trackingNumber)
        {
            try
            {
                var dto = new UpdateOrderStatusDto
                {
                    OrderId = orderId,
                    Status = status,
                    TrackingNumber = trackingNumber
                };

                await _orderService.UpdateOrderStatusAsync(dto);
                TempData["Success"] = "Order status updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // POST: Admin/Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                TempData["Success"] = "Order cancelled successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}