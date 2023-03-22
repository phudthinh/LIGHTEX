using LIGHTEX.Data;
using LIGHTEX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using System.Web;

namespace LIGHTEX.Controllers
{
    public class ChangePasswordController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> Change(ChangePasswordViewModel change)
        {
            if (string.IsNullOrWhiteSpace(change.newpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu mới không được để trống.";
                return View("Index");
            }
            else if (string.IsNullOrWhiteSpace(change.re_newpassword))
            {
                ViewBag.ErrorMessage = "Mật khẩu nhập lại không được để trống.";
                return View("Index");
            }
            else if (change.newpassword != change.re_newpassword)
            {
                ViewBag.ErrorMessage = "Không trùng khớp.";
                return View("Index");
            }
            else
            {
                ViewBag.Username = Request.Query["username"];
                string linkUsername = HttpContext.Request.Query["username"];
                var account = await _context.Account
                    .Where(a => a.username == linkUsername && a.active == true && a.permission == 0)
                    .FirstOrDefaultAsync();
                if (account == null)
                {
                    ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                    return View("Index");
                }
                else
                {
                    account.password = change.newpassword;
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Login");
                }
            }
        }
    }
}
