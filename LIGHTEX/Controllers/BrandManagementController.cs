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
    public class BrandManagementController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BrandManagementController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
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
        public IActionResult Update(int id_brand)
        {
            var existingBrand = _context.Brand.FirstOrDefault(c => c.id_brand == id_brand);

            if (existingBrand == null)
            {
                return NotFound();
            }

            var brand = new Brand()
            {
                id_brand = existingBrand.id_brand,
                name = existingBrand.name,
                description= existingBrand.description,
            };

            return View(brand);
        }
        [HttpPost]
        public async Task<IActionResult> CreateBrand(Brand create)
        {
            if (string.IsNullOrWhiteSpace(create.name))
            {
                ViewBag.ErrorMessage = "Tên nhãn hàng không được để trống.";
                return View("Create");
            }
            else
            {
                var brand = new Brand()
                {
                    name = create.name,
                    description = string.IsNullOrWhiteSpace(create.description) ? "" : create.description,

                };
                await _context.Brand.AddAsync(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "BrandManagement");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateBrand(Brand update)
        {
            var existingBrand = await _context.Brand.FirstOrDefaultAsync(c => c.id_brand == update.id_brand);

            if (string.IsNullOrWhiteSpace(update.name))
            {
                ViewBag.ErrorMessage = "Tên nhãn hàng không được để trống.";
                return View("Update", update);
            }
            else
            {
                existingBrand.name = update.name;
                existingBrand.description = string.IsNullOrWhiteSpace(update.description) ? "" : update.description;

                _context.Brand.Update(existingBrand);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "BrandManagement");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id_brand)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var brand = await _context.Brand.FirstOrDefaultAsync(c => c.id_brand == id_brand);
                    if (brand != null)
                    {
                        _context.Brand.Remove(brand);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    return RedirectToAction("Index", "BrandManagement");
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
