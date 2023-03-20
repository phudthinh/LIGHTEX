using LIGHTEX.Data;
using LIGHTEX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Authorization;

namespace LIGHTEX.Controllers
{
    public class LoginController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LoginController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (string.IsNullOrWhiteSpace(login.username))
            {
                ViewBag.ErrorMessage = "Tài khoản không được để trống.";
                return View();
            }
            else if (string.IsNullOrWhiteSpace(login.password))
            {
                ViewBag.ErrorMessage = "Mật khẩu không được để trống.";
                return View();
            }
            else
            {
                // kiểm tra thông tin đăng nhập được cung cấp cho đúng hay không
                var account = await _context.Account
                    .Where(a => a.username == login.username && a.password == login.password && a.active == true && a.permission == 0)
                    .FirstOrDefaultAsync();
                var admin = await _context.Account
                   .Where(a => a.username == login.username && a.password == login.password && a.active == true && a.permission == 1)
                   .FirstOrDefaultAsync();
                if (account == null && admin == null)
                {
                    // thông tin xác thực sai, trả về một thông báo lỗi
                    ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                    return View();
                }
                else if (account != null)
                {
                    // thông tin xác thực đúng, đăng nhập người dùng
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.username),
                        new Claim(ClaimTypes.Role, "User")
                    };
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        AllowRefresh = true,
                    };

                    await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                    CurrentUser = account;

                    // chuyển hướng đến trang chính
                    return RedirectToAction("Index", "Home");
                }
                else if (admin != null)
                {
                    // thông tin xác thực đúng, đăng nhập người dùng
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.username),
                        new Claim(ClaimTypes.Role, "User")
                    };
                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        AllowRefresh = true,
                    };

                    await _httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                    CurrentUser = admin;

                    // chuyển hướng đến trang quản trị
                    return RedirectToAction("Index", "Admin");
                }
                else { return View(); }
            }
        }
        public async Task LoginGoogle()
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
            // kiểm tra thông tin đăng nhập được cung cấp cho đúng hay không
            var account = await _context.Account
                .Where(a => a.username == emailCustomer && a.password == "Account Google" && a.active == true && a.permission == 0)
                .FirstOrDefaultAsync();
            var admin = await _context.Account
               .Where(a => a.username == emailCustomer && a.password == "Account Google" && a.active == true && a.permission == 1)
               .FirstOrDefaultAsync();
            if (account == null && admin == null)
            {
                // thông tin xác thực sai, trả về một thông báo lỗi
                ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                return View();
            }
            else if (account != null)
            {
                // thông tin xác thực đúng, đăng nhập người dùng
                var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, account.username),
                        new Claim(ClaimTypes.Role, "User")
                    };
                var claimsIdentity = new ClaimsIdentity(
                    userClaims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true,
                };

                await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

                CurrentUser = account;

                // chuyển hướng đến trang chính
                return RedirectToAction("Index", "Home");
            }
            else if (admin != null)
            {
                // thông tin xác thực đúng, đăng nhập người dùng
                var adminClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.username),
                        new Claim(ClaimTypes.Role, "User")
                    };
                var claimsIdentity = new ClaimsIdentity(
                    adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                    AllowRefresh = true,
                };

                await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

                CurrentUser = admin;

                // chuyển hướng đến trang quản trị
                return RedirectToAction("Index", "Admin");
            }
            else { return View(); }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            CurrentUser = null;
            return RedirectToAction("Index", "Login");
        }
        private Account CurrentUser
        {
            get { return HttpContext.Items["CurrentUser"] as Account; }
            set { HttpContext.Items["CurrentUser"] = value; }
        }
    }
}
