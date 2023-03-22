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
using Microsoft.Extensions.Configuration;
using Facebook;
using System.IO;

namespace LIGHTEX.Controllers
{
    public class LoginController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string appID = "";
        private string appSecret = "";

        public LoginController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            var configuration = GetConfiguration();
            appID = configuration.GetSection("Authentication:Facebook:AppID").Value;
            appSecret = configuration.GetSection("Authentication:Facebook:AppSecret").Value;

        }

        private Account CurrentUser
        {
            get { return HttpContext.Items["CurrentUser"] as Account; }
            set { HttpContext.Items["CurrentUser"] = value; }
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
                return View("Index");
            }
            else if (string.IsNullOrWhiteSpace(login.password))
            {
                ViewBag.ErrorMessage = "Mật khẩu không được để trống.";
                return View("Index");
            }
            else
            {
                // kiểm tra thông tin đăng nhập được cung cấp cho đúng hay không
                var account = await _context.Account
                    .Where(a => a.username == login.username && a.password == login.password && a.password != "Account Google" && a.active == true && a.permission == 0)
                    .FirstOrDefaultAsync();
                var admin = await _context.Account
                   .Where(a => a.username == login.username && a.password == login.password && a.password != "Account Google" && a.active == true && a.permission == 1)
                   .FirstOrDefaultAsync();
                var accountGoogle = await _context.Account
                    .Where(a => a.username == login.username && a.password == login.password && a.password == "Account Google" && a.active == true && a.permission == 0)
                    .FirstOrDefaultAsync();
                var adminGoogle = await _context.Account
                   .Where(a => a.username == login.username && a.password == login.password && a.password == "Account Google" && a.active == true && a.permission == 1)
                   .FirstOrDefaultAsync();
                var accountFacebook = await _context.Account
                    .Where(a => a.username == login.username && a.password == login.password && a.password == "Account Facebook" && a.active == true && a.permission == 0)
                    .FirstOrDefaultAsync();
                var adminFacebook = await _context.Account
                   .Where(a => a.username == login.username && a.password == login.password && a.password == "Account Facebook" && a.active == true && a.permission == 1)
                   .FirstOrDefaultAsync();
                if (accountGoogle != null || adminGoogle != null)
                {
                    // thông tin xác thực sai, trả về một thông báo lỗi
                    ViewBag.ErrorMessage = "Vui lòng đăng nhập bằng biểu tượng Google.";
                    return View("Index");
                }
                else if (accountFacebook != null || adminFacebook != null)
                {
                    // thông tin xác thực sai, trả về một thông báo lỗi
                    ViewBag.ErrorMessage = "Vui lòng đăng nhập bằng biểu tượng Facebook.";
                    return View("Index");
                }
                else if (account == null && admin == null)
                {
                    // thông tin xác thực sai, trả về một thông báo lỗi
                    ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                    return View("Index");
                }
                else if (account != null && accountGoogle == null)
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
                    account.last_login = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }
                else if (admin != null && adminGoogle == null)
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
                    admin.last_login = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return View("Index");
                }
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
                return View("Index");
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
                account.last_login = DateTime.Now;
                await _context.SaveChangesAsync();

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
                admin.last_login = DateTime.Now;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Admin");
            }
            else
            { 
                return View("Index");
            }
        }
        public IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
        private Uri RedirectUri
        {
            get
            {
                var requestUrl = $"{Request.Scheme}://{Request.Host}";
                var callbackUrl = Url.Action("FacebookCallback");
                return new Uri(requestUrl + callbackUrl);
            }
        }

        public async Task<IActionResult> LoginFacebook()
        {
            var fb = new FacebookClient();
            var loginurl = fb.GetLoginUrl(new
            {
                client_id = appID,
                client_secret = appSecret,
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });
            return Redirect(loginurl.AbsoluteUri);
        }
        public async Task<IActionResult> FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = appID,
                client_secret = appSecret,
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });
            var accesstoken = result.access_token;
            fb.AccessToken = accesstoken;
            dynamic data = fb.Get("me?fields=link,first_name,last_name,email,picture");
            TempData["email"] = data.email;
            TempData["name"] = data.first_name + " " + data.last_name;
            TempData["picture"] = data.picture.data.url;
            string nameCustomer = data.email;
            string emailCustomer = data.email;
            string avatarCustomerUrl = data.picture.data.url;
            var account = await _context.Account
                .Where(a => a.username == emailCustomer && a.password == "Account Facebook" && a.active == true && a.permission == 0)
                .FirstOrDefaultAsync();
            var admin = await _context.Account
               .Where(a => a.username == emailCustomer && a.password == "Account Facebook" && a.active == true && a.permission == 1)
               .FirstOrDefaultAsync();
             if (account == null && admin == null)
            {
                // thông tin xác thực sai, trả về một thông báo lỗi
                ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                return View("Index");
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
                account.last_login = DateTime.Now;
                await _context.SaveChangesAsync();

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
                admin.last_login = DateTime.Now;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Admin");
            }
            else
            {
                return View("Index");
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            CurrentUser = null;
            return RedirectToAction("Index", "Login");
        }
    }
}
