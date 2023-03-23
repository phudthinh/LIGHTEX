using LIGHTEX.Data;
using LIGHTEX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;

namespace LIGHTEX.Controllers
{
    public class ForgotPasswordController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ForgotPasswordController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> Submit(LoginViewModel login)
        {
            if (string.IsNullOrWhiteSpace(login.username))
            {
                ViewBag.ErrorMessage = "Tài khoản không được để trống.";
                return View("Index");
            }
            else
            {
                var account = await _context.Account
                    .Where(a => a.username == login.username && a.active == true && a.permission == 0)
                    .FirstOrDefaultAsync();
                var customer = await _context.Customer
                    .Where(a => a.username == login.username)
                    .FirstOrDefaultAsync();
                if (account == null)
                {
                    ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                    return View("Index");
                }
                else if (string.IsNullOrWhiteSpace(customer.email))
                {
                    ViewBag.ErrorMessage = "Tài khoản không có địa chỉ email để khôi phục. Vui lòng liên hệ nhà phát triển";
                    return View("Index");
                }
                else
                {
                    HttpContext.Session.SetString("CustomerEmail", customer.email);
                    var customerEmail = HttpContext.Session.GetString("CustomerEmail");
                    ViewBag.CustomerEmail = customerEmail;
                    var emailService = new EmailService();
                    var callbackUrl = Url.Action("Index", "ChangePassword", new { username = customer.username }, protocol: Request.Scheme);
                    var emailBody = $"Chúng tôi đã nhận được yêu cầu thay đổi mật khẩu của bạn. Vui lòng nhấn vào liên kết để đổi mật khẩu: <a href='{callbackUrl}'>Đổi mật khẩu</a>";
                    await emailService.SendEmailAsync(customer.email, "Xác nhận đổi mật khẩu Lightex của bạn", emailBody);
                    return View("Index");
                }
            }
        }
        public class EmailService
        {
            public async Task SendEmailAsync(string email, string subject, string message)
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress("Lightex", "phuthinhmtp@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587);
                    await client.AuthenticateAsync("phuthinhmtp@gmail.com", "ixftzhukvtguvhxo");
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
