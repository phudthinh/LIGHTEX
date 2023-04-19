using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Security.Cryptography;
using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text;

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
        public IActionResult Cart()
        {
            return View();
        }
        public IActionResult Order()
        {
            return View();
        }
        public IActionResult Thanks()
        {
            return View();
        }
        public IActionResult Comment(int id_bill)
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
            var comment = new CommentViewModel()
            {
                id_bill= id_bill,
                product = _product.name,
                category = _category.name,
                brand = _brand.name,
                image = _product.image,
            };

            return View(comment);
        }
        public async Task<IActionResult> CommentSend(CommentViewModel send)
        {
            var _bill = await _context.Bill.FirstOrDefaultAsync(c => c.id_bill == send.id_bill);
            var _customer = _context.Customer.FirstOrDefault(c => c.id_customer == _bill.id_customer);
            var _account = _context.Account.FirstOrDefault(c => c.username == _customer.username);
            var _product = _context.Product.FirstOrDefault(p => p.id_product == _bill.id_product);
            var _category = _context.Category.FirstOrDefault(c => c.id_category == _product.id_category);
            var _brand = _context.Brand.FirstOrDefault(b => b.id_brand == _product.id_brand);
            var _comment = new CommentViewModel()
            {
                id_bill = send.id_bill,
                product = _product.name,
                category = _category.name,
                brand = _brand.name,
                image = _product.image,
            };
            if (_bill == null)
            {
                return NotFound();
            }
            else if (send.star == 0)
            {
                ViewBag.ErrorMessage = "Vui lòng chọn số sao để đánh giá.";
                return View("Comment", _comment);
            }
            else if (string.IsNullOrWhiteSpace(send.content))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập nội dung đánh giá.";
                return View("Comment", _comment);
            }
            else
            {
                var comment = new Comment()
                {
                    id_customer = _bill.id_customer,
                    id_product = _bill.id_product,
                    content = send.content,
                    star = send.star
                };
                _context.Comment.AddAsync(comment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult GetCartCount(int id)
        {
            var cartCount = _context.Cart.Where(c => c.id_customer == id).Count();
            return Json(cartCount);
        }
        public async Task<IActionResult> GetCartList(int id_customer)
        {
            var carts = await _context.Cart
            .Where(c => c.id_customer == id_customer)
            .Select(c => new
            {
                Cart = c,
                Product = _context.Product.FirstOrDefault(p => p.id_product == c.id_product)
            })
            .ToListAsync();
            return Json(carts);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCart(int id_cart)
        {
            var cart = await _context.Cart.FindAsync(id_cart);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Cart.Remove(cart);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CancelBill(int id_bill)
        {
            var bill = await _context.Bill.FindAsync(id_bill);
            var product = await _context.Product.FirstOrDefaultAsync(c => c.id_product == bill.id_product);
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.id_customer == bill.id_customer);
            if (bill == null)
            {
                return NotFound();
            }
            bill.status = 3;
            if (bill.payments == 1)
            {
                customer.money = customer.money + (bill.quantity * product.price);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> CheckoutAsync(int[] selectedCarts, int[] quantities, int payments, int id_customer)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.id_customer == id_customer);
            var totalBill = 0;

            if (selectedCarts.Length > 0)
            {
                if (payments == 0)
                {
                    for (int i = 0; i < selectedCarts.Length; i++)
                    {
                        int cartId = selectedCarts[i];
                        var cart = _context.Cart.FirstOrDefault(c => c.id_cart == cartId);
                        var product = _context.Product.FirstOrDefault(c => c.id_product == cart.id_product);
                        if (cart == null)
                        {
                            continue;
                        }
                        totalBill = (int)(totalBill + product.price * quantities[i]);
                        var bill = new Bill
                        {
                            id_customer = cart.id_customer,
                            id_product = cart.id_product,
                            quantity = quantities[i],
                            payments = payments,
                            status = 0,
                            ship_date = DateTime.Now.AddDays(3),
                            create_date = DateTime.Now
                        };
                        _context.Bill.Add(bill);
                        _context.Cart.Remove(cart);
                    }
                    _context.SaveChanges();
                    return RedirectToAction("Thanks", "Home");
                }
                else if (payments == 1)
                {
                    for (int i = 0; i < selectedCarts.Length; i++)
                    {
                        int cartId = selectedCarts[i];
                        var cart = _context.Cart.FirstOrDefault(c => c.id_cart == cartId);
                        var product = _context.Product.FirstOrDefault(c => c.id_product == cart.id_product);
                        if (cart == null)
                        {
                            continue;
                        }
                        totalBill = (int)(totalBill + product.price * quantities[i]);
                    }
                    if (customer.money >= (double)totalBill)
                    {
                        for (int i = 0; i < selectedCarts.Length; i++)
                        {
                            int cartId = selectedCarts[i];
                            var cart = _context.Cart.FirstOrDefault(c => c.id_cart == cartId);
                            var product = _context.Product.FirstOrDefault(c => c.id_product == cart.id_product);
                            if (cart == null)
                            {
                                continue;
                            }
                            var bill = new Bill
                            {
                                id_customer = cart.id_customer,
                                id_product = cart.id_product,
                                quantity = quantities[i],
                                payments = payments,
                                status = 0,
                                ship_date = DateTime.Now.AddDays(3),
                                create_date = DateTime.Now
                            };
                            _context.Bill.Add(bill);
                            _context.Cart.Remove(cart);
                        }
                        customer.money = customer.money - (double)totalBill;
                        _context.SaveChanges();
                        return RedirectToAction("Thanks", "Home");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Số dư của quý khách không đủ.";
                        return View("Cart");
                    }
                }
                else
                {
                    return View("Cart");
                }
            }
            return View("Cart");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(CustomerViewModel update, IFormFile image)
        {
            var existingAccount = await _context.Account.FirstOrDefaultAsync(a => a.username == update.username);
            var existingCustomer = await _context.Customer.FirstOrDefaultAsync(a => a.username == update.username);
            var existingEmail = await _context.Customer.FirstOrDefaultAsync(a => a.email == update.email && a.username != update.username && (update.email == null || a.email != null));

            if (string.IsNullOrWhiteSpace(update.full_name))
            {
                ViewBag.ErrorMessage = "Tên người dùng không được để trống.";
                return View("Information", update);
            }

            else if (existingEmail != null)
            {
                ViewBag.ErrorMessage = "Email này đã được sử dụng.";
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
            if (string.IsNullOrWhiteSpace(change.oldpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu hiện tại không được để trống.";
                return View("ChangePassword");
            }
            else if (string.IsNullOrWhiteSpace(change.newpassword))
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
                var sha256 = SHA256.Create();
                var oldpasswordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(change.oldpassword));
                var oldpasswordHashString = BitConverter.ToString(oldpasswordHash).Replace("-", "").ToLower();

                var newpasswordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(change.newpassword));
                var newpasswordHashString = BitConverter.ToString(newpasswordHash).Replace("-", "").ToLower();

                var account = await _context.Account.FirstOrDefaultAsync(a => a.username == change.username && a.password == oldpasswordHashString);
                if (account == null)
                {
                    ViewBag.ErrorMessage = "Mật khẩu hiện tại không đúng.";
                    return View("ChangePassword");
                }
                else
                {
                    account.password = newpasswordHashString;
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
                    return RedirectToAction("Pay", "Home");
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