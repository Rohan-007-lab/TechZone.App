using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;

namespace TechZone.Web.Controllers
{
    [Authorize]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
        }

        // GET: Orders/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Order not found";
                return RedirectToAction(nameof(MyOrders));
            }

            // Check if user owns this order
            if (order.UserId != user.Id)
            {
                TempData["Error"] = "Unauthorized access";
                return RedirectToAction(nameof(MyOrders));
            }

            return View(order);
        }

        // GET: Orders/Checkout
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetUserCartAsync(user.Id);
            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            // Create checkout model with user info
             var model = new CreateOrderDto
             {
                 UserId = user.Id,
                 ShippingFirstName = user.FirstName,
                 ShippingLastName = user.LastName,
                 ShippingEmail = user.Email ?? "",
                 ShippingPhone = user.PhoneNumber ?? "",
                 BillingFirstName = user.FirstName,
                 BillingLastName = user.LastName
             };

             ViewBag.Cart = cart;
             return View(model);
         }

            // POST: Orders/Checkout
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Checkout(CreateOrderDto model)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                        return RedirectToAction("Login", "Account");

                    var cart = await _cartService.GetUserCartAsync(user.Id);
                    if (cart == null || !cart.CartItems.Any())
                    {
                        TempData["Error"] = "Your cart is empty";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Populate order items from cart
                    model.UserId = user.Id;
                model.OrderItems = cart.CartItems.Select(item => new CreateOrderDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList();

                    // Create order
                    var order = await _orderService.CreateOrderAsync(model);

                    // Clear cart after successful order
                    await _cartService.ClearCartAsync(user.Id);

                    TempData["Success"] = "Order placed successfully!";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    ViewBag.Cart = await _cartService.GetUserCartAsync(model.UserId);
                    return View(model);
                }
            }

            // POST: Orders/Cancel/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Cancel(int id)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                        return RedirectToAction("Login", "Account");

                    var order = await _orderService.GetOrderByIdAsync(id);
                    if (order == null || order.UserId != user.Id)
                    {
                        TempData["Error"] = "Order not found";
                        return RedirectToAction(nameof(MyOrders));
                    }

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