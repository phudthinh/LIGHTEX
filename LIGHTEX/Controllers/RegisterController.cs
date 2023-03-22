using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Facebook;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace LIGHTEX.Controllers
{
    public class RegisterController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string appID = "";
        private string appSecret = "";

        public RegisterController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            var configuration = GetConfiguration();
            appID = configuration.GetSection("Authentication:Facebook:AppID").Value;
            appSecret = configuration.GetSection("Authentication:Facebook:AppSecret").Value;
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
                return View("Index");
            }
            else if (string.IsNullOrWhiteSpace(register.password))
            {
                ViewBag.ErrorMessage = "Mật khẩu không được để trống.";
                return View("Index");
            }
            else if (string.IsNullOrWhiteSpace(register.full_name))
            {
                ViewBag.ErrorMessage = "Tên của bạn không được để trống.";
                return View("Index");
            }
            else
            {
                // Kiểm tra xem tài khoản đã tồn tại chưa
                var existingUser = await _context.Account.FirstOrDefaultAsync(u => u.username == register.username);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "Tài khoản này đã được sử dụng.";
                    return View("Index");
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
            var avatarClaim = result.Principal.FindFirst("urn:google:picture");
            string nameCustomer = nameClaim.Value;
            string emailCustomer = emailClaim.Value;
            string avatarCustomerUrl = avatarClaim.Value;

            var httpClient = new HttpClient();
            var avatarBytes = await httpClient.GetByteArrayAsync(avatarCustomerUrl);

            var existingUser = await _context.Account.FirstOrDefaultAsync(u => u.username == emailCustomer);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Tài khoản này đã được sử dụng.";
                return View("Index");
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

        public async Task<IActionResult> RegisterFacebook()
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
            var httpClient = new HttpClient();
            var avatarBytes = await httpClient.GetByteArrayAsync(avatarCustomerUrl);

            var existingUser = await _context.Account.FirstOrDefaultAsync(u => u.username == emailCustomer);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Tài khoản này đã được sử dụng.";
                return View("Index");
            }
            else
            {
                var account = new Account()
                {
                    username = emailCustomer,
                    password = "Account Facebook",
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
