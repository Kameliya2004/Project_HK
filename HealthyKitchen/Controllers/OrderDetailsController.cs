using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HealthyKitchen.Data;
using HealthyKitchen.Models;

namespace HealthyKitchen.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public const string OrderSession = "OrderId";

        public OrderDetailsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            TempData.Keep("OrderActive");

            var orderId = GetOrderId();
            if (orderId == null)
            {
                return RedirectToAction("Index", "Products");
            }
            var currentUser = _userManager.GetUserId(User);
            var applicationDbContext = _context.OrderDetails
                .Include(p => p.Product)
                .Include(o => o.Order)
                .Where(x => (x.OrderId == orderId) &&
                            (x.Order.Finalised == false) &&
                            (x.Order.UserId == currentUser)); 

            return View(await applicationDbContext.ToListAsync());
           
        }
        public async Task<IActionResult> Calculate(int orderId)
        {
            var currentUser = _userManager.GetUserId(User);
            var dbOrderList = _context.OrderDetails
               .Include(p => p.Product)
               .Include(o => o.Order)
               .Where(x => (x.OrderId == orderId) &&
                           (x.Order.Finalised == false) &&
                           (x.Order.UserId == currentUser));
            double sum = 0;
            foreach (var item in dbOrderList)
            {
                sum += (item.Product.Price * item.Quantity);
            }
            Order order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            order.Finalised = true;
            order.Total = sum;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("OrderSession");
            TempData["OrderActive"] = false;

            TempData["Message"] = "Успешно поръчахте на стойност " + sum.ToString() + "лв.";
            return RedirectToAction("Index", "Products");
        }
        [NonAction]
        public int? GetOrderId()
        {
            return HttpContext.Session.GetInt32("OrderSession");
        }

        
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM product)
        {
            TempData.Keep("OrderActive");

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
                return View();
            }
            if (GetOrderId() == null)
            {
                Order order = new Order()
                {
                    UserId = _userManager.GetUserId(User),
                    OrderedOn = DateTime.Now
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetInt32("OrderSession", order.Id);
                TempData["Message"] = "Имате поръчка, която не е завършена!";
                TempData["OrderActive"] = true;
            }

            int shopCartId = (int)GetOrderId();
            var orderItem = await _context.OrderDetails.SingleOrDefaultAsync(i => (i.ProductId == product.Id && i.OrderId == shopCartId));
            if (orderItem == null)
            {
                orderItem = new OrderDetails()
                {
                    ProductId = product.Id, 
                    Quantity = product.Quantity,
                    OrderId = (int)GetOrderId()
                };
                _context.OrderDetails.Add(orderItem);
            }
            else
            {
                orderItem.Quantity = orderItem.Quantity + product.Quantity;
                _context.OrderDetails.Update(orderItem);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }
    }
}
