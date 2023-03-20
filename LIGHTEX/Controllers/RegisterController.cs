using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Numerics;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace LIGHTEX.Controllers
{
    public class RegisterController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RegisterController(LIGHTEXContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel register)
        {
            if (string.IsNullOrWhiteSpace(register.username))
            {
                ViewBag.ErrorMessage = "Tài khoản không được để trống.";
                return View();
            }
            else if (string.IsNullOrWhiteSpace(register.password))
            {
                ViewBag.ErrorMessage = "Mật khẩu không được để trống.";
                return View();
            }
            else if (string.IsNullOrWhiteSpace(register.full_name))
            {
                ViewBag.ErrorMessage = "Tên của bạn không được để trống.";
                return View();
            }
            else
            {
                // Kiểm tra xem tài khoản đã tồn tại chưa
                var existingUser = await _context.Account.FirstOrDefaultAsync(u => u.username == register.username);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "Tài khoản này đã được sử dụng.";
                    return View();
                }
                else
                {
                    var account = new Account()
                    {
                        username = register.username,
                        password = register.password,
                        full_name = register.full_name,
                        active = true,
                        permission = 0,
                        last_login = DateTime.Now,
                        create_date = DateTime.Now

                    };
                    var customer = new Customer()
                    {
                        username = register.username,
                        email = "",
                        phone = "",
                        address = "",
                        ward = "",
                        city = "",
                        avatar = new byte[0],
                        money = 0,
                    };
                    await _context.Account.AddAsync(account);
                    await _context.Customer.AddAsync(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Login");
                }
            }
        }
        public async Task RegisterGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities
                .FirstOrDefault().Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });
            var nameClaim = result.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            var emailClaim = result.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

            string nameCustomer = nameClaim.Value;
            string emailCustomer = emailClaim.Value;

            var avatarClaim = result.Principal.FindFirst("urn:google:picture");
            string avatarCustomerUrl = avatarClaim.Value;

            // Khởi tạo HttpClient
            var httpClient = new HttpClient();

            // Lấy mảng byte của ảnh từ đường link
            var avatarBytes = await httpClient.GetByteArrayAsync(avatarCustomerUrl);
            // Kiểm tra xem tài khoản đã tồn tại chưa
            var existingUser = await _context.Account.FirstOrDefaultAsync(u => u.username == emailCustomer);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Tài khoản này đã được sử dụng.";
                return View();
            }
            else
            {
                var account = new Account()
                {
                    username = emailCustomer,
                    password = "Account Google",
                    full_name = nameCustomer,
                    active = true,
                    permission = 0,
                    last_login = DateTime.Now,
                    create_date = DateTime.Now

                };
                var customer = new Customer()
                {
                    username = emailCustomer,
                    email = emailCustomer,
                    phone = "",
                    address = "",
                    ward = "",
                    city = "",
                    avatar = avatarBytes,
                    money = 0,
                };
                await _context.Account.AddAsync(account);
                await _context.Customer.AddAsync(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Login");
            }
        }
    }
}
