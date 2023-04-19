using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LIGHTEX.Controllers
{
    public class CustomerManagementController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerManagementController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Update(string username, string email)
        {
            var existingAccount =  _context.Account.FirstOrDefault(a => a.username == username);
            var existingCustomer = _context.Customer.FirstOrDefault(a => a.username == username);
            if (existingAccount == null || existingCustomer == null)
            {
                return NotFound();
            }
            var customer = new CustomerViewModel()
            {
                username = existingAccount.username,
                password = existingAccount.password,
                full_name = existingAccount.full_name,
                active = existingAccount.active,
                email = existingCustomer.email,
                phone = existingCustomer.phone,
                address = existingCustomer.address,
                ward = existingCustomer.ward,
                city = existingCustomer.city,
                money= existingCustomer.money,
            };

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CustomerViewModel create, IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(create.username))
            {
                ViewBag.ErrorMessage = "Tài khoản không được để trống.";
                return View("Create");
            }
            else if (string.IsNullOrWhiteSpace(create.password))
            {
                ViewBag.ErrorMessage = "Mật khẩu không được để trống.";
                return View("Create");
            }
            else if (string.IsNullOrWhiteSpace(create.full_name))
            {
                ViewBag.ErrorMessage = "Tên người dùng không được để trống.";
                return View("Create");
            }
            else
            {
                var existingAccount = await _context.Account.FirstOrDefaultAsync(a => a.username == create.username);
                var existingCustomer = await _context.Customer.FirstOrDefaultAsync(a => a.username == create.username);
                var existingEmail = await _context.Customer.FirstOrDefaultAsync(a => a.email == create.email && a.username != create.username && (create.email == null || a.email != null));

                if (existingAccount != null && existingCustomer != null)
                {
                    ViewBag.ErrorMessage = "Tài khoản này đã được sử dụng.";
                    return View("Create");
                }
                else if (string.IsNullOrWhiteSpace(create.email) && existingEmail != null)
                {
                    ViewBag.ErrorMessage = "Email này đã được sử dụng.";
                    return View("Create");
                }
                else
                {
                    var sha256 = SHA256.Create();
                    var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(create.password));
                    var passwordHashString = BitConverter.ToString(passwordHash).Replace("-", "").ToLower();
                    var account = new Account()
                    {
                        username = create.username,
                        password = passwordHashString,
                        full_name = create.full_name,
                        active = true,
                        permission = 0,
                        create_date = DateTime.Now,
                        last_login = DateTime.Now
                    };

                    var customer = new Customer()
                    {
                        username = create.username,
                        email = string.IsNullOrWhiteSpace(create.email) ? "" : create.email,
                        phone = string.IsNullOrWhiteSpace(create.phone) ? "" : create.phone,
                        address = string.IsNullOrWhiteSpace(create.address) ? "" : create.address,
                        ward = string.IsNullOrWhiteSpace(create.ward) ? "" : create.ward,
                        city = string.IsNullOrWhiteSpace(create.city) ? "" : create.city,
                        money = 0
                    };

                    if (image == null)
                    {
                        customer.avatar = new byte[0];
                    }
                    else
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await image.CopyToAsync(memoryStream);
                            customer.avatar = memoryStream.ToArray();
                        }
                    }

                    await _context.Account.AddAsync(account);
                    await _context.SaveChangesAsync();
                    await _context.Customer.AddAsync(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "CustomerManagement");
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(CustomerViewModel update, IFormFile image)
        {
            var existingAccount = await _context.Account.FirstOrDefaultAsync(a => a.username == update.username);
            var existingCustomer = await _context.Customer.FirstOrDefaultAsync(a => a.username == update.username);
            var existingEmail = await _context.Customer.FirstOrDefaultAsync(a => a.email == update.email && a.username != update.username && (update.email == null || a.email != null));

            if (string.IsNullOrWhiteSpace(update.password))
            {
                ViewBag.ErrorMessage = "Mật khẩu không được để trống.";
                return View("Update", update);
            }
            else if (string.IsNullOrWhiteSpace(update.full_name))
            {
                ViewBag.ErrorMessage = "Tên người dùng không được để trống.";
                return View("Update", update);
            }
            else if (string.IsNullOrWhiteSpace(update.email) && existingEmail != null)
            {
                ViewBag.ErrorMessage = "Email này đã được sử dụng.";
                return View("Update", update);
            }
            else
            {
                var sha256 = SHA256.Create();
                var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(update.password));
                var passwordHashString = BitConverter.ToString(passwordHash).Replace("-", "").ToLower();

                existingAccount.password = passwordHashString;
                existingAccount.full_name = update.full_name;
                existingAccount.active = update.active;
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

                return RedirectToAction("Index", "CustomerManagement");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(string username)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var customer = await _context.Customer.FirstOrDefaultAsync(c => c.username == username);
                    var account = await _context.Account.FirstOrDefaultAsync(a => a.username == username);
                    if (account != null && customer != null)
                    {
                        _context.Customer.Remove(customer);
                        await _context.SaveChangesAsync();

                        _context.Account.Remove(account);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                    }
                    return RedirectToAction("Index", "CustomerManagement");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
