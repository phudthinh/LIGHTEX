using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LIGHTEX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public HomeController(ILogger<HomeController> logger, LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Information()
        {
            return View();
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        public IActionResult Pay()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(CustomerViewModel update, IFormFile image)
        {
            var existingAccount = await _context.Account.FirstOrDefaultAsync(a => a.username == update.username);
            var existingCustomer = await _context.Customer.FirstOrDefaultAsync(a => a.username == update.username);

            if (string.IsNullOrWhiteSpace(update.full_name))
            {
                ViewBag.ErrorMessage = "Tên người dùng không được để trống.";
                return View("Information", update);
            }
            else
            {
                existingAccount.full_name = update.full_name;
                existingCustomer.email = string.IsNullOrWhiteSpace(update.email) ? "" : update.email;
                existingCustomer.phone = string.IsNullOrWhiteSpace(update.phone) ? "" : update.phone;
                existingCustomer.address = string.IsNullOrWhiteSpace(update.address) ? "" : update.address;
                existingCustomer.ward = string.IsNullOrWhiteSpace(update.ward) ? "" : update.ward;
                existingCustomer.city = string.IsNullOrWhiteSpace(update.city) ? "" : update.city;

                if (image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        existingCustomer.avatar = memoryStream.ToArray();
                    }
                }

                _context.Account.Update(existingAccount);
                _context.Customer.Update(existingCustomer);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel change)
        {            
            if (string.IsNullOrWhiteSpace(change.newpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu mới không được để trống.";
                return View("ChangePassword");
            }
            else if (string.IsNullOrWhiteSpace(change.re_newpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu nhập lại không được để trống.";
                return View("ChangePassword");
            }
            else if (change.newpassword != change.re_newpassword)
            {
                ViewBag.ErrorMessage = "Không trùng khớp.";
                return View("ChangePassword");
            }
            else
            {
                var account = await _context.Account.FirstOrDefaultAsync(a => a.username == change.username && a.password == change.oldpassword);
                if (account == null)
                {
                    ViewBag.ErrorMessage = "Mật khẩu hiện tại không đúng.";
                    return View("ChangePassword");
                }
                else
                {
                    account.password = change.newpassword;
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> Pay(CustomerViewModel pay)
        {
            if (pay.money <= 0)
            {
                ViewBag.ErrorMessage = "Không được để tiền trống hoặc bé hơn 0.";
                return View("Pay");
            }
            else
            {
                var customer = await _context.Customer.FirstOrDefaultAsync(c => c.username == pay.username);
                double newMoney = customer.money + pay.money;
                if (customer == null)
                {
                    ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                    return View("Pay");
                }
                else
                {
                    customer.money = newMoney;
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        [HttpPost]
        public async Task<JsonResult> AddToCartAsync(Cart cart)
        {
            var checkCart = await _context.Cart.FirstOrDefaultAsync(c => c.id_product == cart.id_product && c.id_customer == cart.id_customer);
            if (checkCart != null)
            {
                // Sản phẩm đã có trong giỏ hàng
                checkCart.quantity += cart.quantity;
                _context.Cart.Update(checkCart);
            }
            else
            {
                // Sản phẩm chưa có trong giỏ hàng
                var cartItem = new Cart
                {
                    id_customer = cart.id_customer,
                    id_product = cart.id_product,
                    quantity = cart.quantity
                };
                _context.Cart.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int customerId, int productId, bool active)
        {
            var favorite = await _context.Favorite.FirstOrDefaultAsync(f => f.id_customer == customerId && f.id_product == productId);
            if (favorite == null)
            {
                // Nếu không tìm thấy dữ liệu yêu thích, tạo mới đối tượng Favorite
                favorite = new Favorite()
                {
                    id_customer = customerId,
                    id_product = productId,
                    active = true
                };
                _context.Favorite.Add(favorite);
            }
            else
            {
                // Cập nhật trạng thái yêu thích nếu tìm thấy đối tượng Favorite
                favorite.active = active;
                _context.Entry(favorite).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        public JsonResult CheckFavoriteStatus(int customerId, int productId)
        {
            var favorite = _context.Favorite.FirstOrDefault(f => f.id_customer == customerId && f.id_product == productId);
            bool isFavorited = favorite != null ? favorite.active : false;

            return Json(new { isFavorited = isFavorited });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}