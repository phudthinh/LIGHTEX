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
    public class CategoryManagementController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoryManagementController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
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
        public IActionResult Update(int id_category)
        {
            var existingCategory = _context.Category.FirstOrDefault(c => c.id_category == id_category);

            if (existingCategory == null)
            {
                return NotFound();
            }

            var category = new Category()
            {
                id_category = existingCategory.id_category,
                name = existingCategory.name,
                description= existingCategory.description,
            };

            return View(category);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category create, IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(create.name))
            {
                ViewBag.ErrorMessage = "Tên loại sản phẩm không được để trống.";
                return View("Create");
            }
            else
            {
                var category = new Category()
                {
                    name = create.name,
                    description = string.IsNullOrWhiteSpace(create.description) ? "" : create.description,

                };
                if (image == null)
                {
                    ViewBag.ErrorMessage = "Hình ảnh không được để trống.";
                    return View("Create");
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        category.icon = memoryStream.ToArray();
                    }
                }

                await _context.Category.AddAsync(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "CategoryManagement");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category update, IFormFile image)
        {
            var existingCategory = await _context.Category.FirstOrDefaultAsync(c => c.id_category == update.id_category);

            if (string.IsNullOrWhiteSpace(update.name))
            {
                ViewBag.ErrorMessage = "Tên loại sản phẩm không được để trống.";
                return View("Update", update);
            }
            else
            {
                existingCategory.name = update.name;
                existingCategory.description = string.IsNullOrWhiteSpace(update.description) ? "" : update.description;

                if (image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        existingCategory.icon = memoryStream.ToArray();
                    }
                }

                _context.Category.Update(existingCategory);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "CategoryManagement");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id_category)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var category = await _context.Category.FirstOrDefaultAsync(c => c.id_category == id_category);
                    if (category != null)
                    {
                        // Kiểm tra xem danh mục này có liên kết với bất kỳ sản phẩm nào không
                        if (!_context.Product.Any(p => p.id_category == id_category))
                        {
                            // Nếu không có sản phẩm nào liên kết với danh mục này thì xóa danh mục
                            _context.Category.Remove(category);
                            await _context.SaveChangesAsync();
                            transaction.Commit();
                        }
                        else
                        {
                            // Nếu có sản phẩm liên kết với danh mục này, trả về một thông báo lỗi
                            return BadRequest("Không thể xóa danh mục vì vẫn còn sản phẩm liên kết.");
                        }
                    }
                    return RedirectToAction("Index", "CategoryManagement");
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
