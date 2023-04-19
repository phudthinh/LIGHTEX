using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;

namespace LIGHTEX.Controllers
{
    public class OrderManagementController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderManagementController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail(int id_bill)
        {
            var _bill = _context.Bill.FirstOrDefault(b => b.id_bill == id_bill);
            var _customer = _context.Customer.FirstOrDefault(c => c.id_customer == _bill.id_customer);
            var _account = _context.Account.FirstOrDefault(c => c.username == _customer.username);
            var _product = _context.Product.FirstOrDefault(p => p.id_product == _bill.id_product);
            var _category = _context.Category.FirstOrDefault(c => c.id_category == _product.id_category);
            var _brand = _context.Brand.FirstOrDefault(b => b.id_brand == _product.id_brand);
            if (_bill == null)
            {
                return NotFound();
            }

            var orderDetail = new OrderDetailViewModel()
            {
                id_bill = _bill.id_bill,
                full_name = _account.full_name,
                username = _account.username,
                product = _product.name,
                category = _category.name,
                brand = _brand.name,
                image = _product.image,
                price = _product.price,
                quantity = _bill.quantity,
                create_date = _bill.create_date,
                ship_date = _bill.ship_date,
                status = _bill.status,
                payments= _bill.payments,
            };
            return View(orderDetail);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDetail(OrderDetailViewModel update)
        {
            var deltailBill = await _context.Bill.FirstOrDefaultAsync(c => c.id_bill == update.id_bill);
            var _bill = _context.Bill.FirstOrDefault(b => b.id_bill == update.id_bill);
            var _customer = _context.Customer.FirstOrDefault(c => c.id_customer == _bill.id_customer);
            var _account = _context.Account.FirstOrDefault(c => c.username == _customer.username);
            var _product = _context.Product.FirstOrDefault(p => p.id_product == _bill.id_product);
            var _category = _context.Category.FirstOrDefault(c => c.id_category == _product.id_category);
            var _brand = _context.Brand.FirstOrDefault(b => b.id_brand == _product.id_brand);

            deltailBill.status = update.status;
            deltailBill.ship_date = update.ship_date;
            if (update.payments == 1 && update.status == 4)
            {
                _customer.money = _customer.money + (_bill.quantity * _product.price);
                _context.Bill.Update(deltailBill);
            }
            await _context.SaveChangesAsync();
            _context.Customer.Update(_customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "OrderManagement");

        }
    }
}
