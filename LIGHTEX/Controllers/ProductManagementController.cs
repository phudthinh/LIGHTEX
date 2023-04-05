using LIGHTEX.Data;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.IO;

namespace LIGHTEX.Controllers
{
    public class ProductManagementController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductManagementController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
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
        public IActionResult Update(int id_product)
        {
            var existingProduct = _context.Product.FirstOrDefault(p => p.id_product == id_product);

            if (existingProduct == null)
            {
                return NotFound();
            }

            var Product = new Product()
            {
                id_product = existingProduct.id_product,
                id_category = existingProduct.id_category,
                id_brand = existingProduct.id_brand,
                name = existingProduct.name,
                information= existingProduct.information,
                price= existingProduct.price,
                status = existingProduct.status,
                modified_date = DateTime.Now,
            };

            return View(Product);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product create, IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(create.name))
            {
                ViewBag.ErrorMessage = "Tên sản phẩm không được để trống.";
                return View("Create");
            }
            else if (create.price == 0)
            {
                ViewBag.ErrorMessage = "Giá tiền không được để trống.";
                return View("Create");
            }
            else if (create.price < 0)
            {
                ViewBag.ErrorMessage = "Giá tiền không âm.";
                return View("Create");
            }
            else if (create.id_category == 0)
            {
                ViewBag.ErrorMessage = "Bạn phải chọn một loại sản phẩm.";
                return View("Create");
            }
            else if (create.id_brand == 0)
            {
                ViewBag.ErrorMessage = "Bạn phải chọn một nhãn hàng.";
                return View("Create");
            }
            else
            {
                var product = new Product()
                {
                    name = create.name,
                    id_category = create.id_category,
                    id_brand = create.id_brand,
                    information = string.IsNullOrWhiteSpace(create.information) ? "" : create.information,
                    price = create.price,
                    status = true,
                    modified_date = DateTime.Now,
                    create_date= DateTime.Now,
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
                        product.image = memoryStream.ToArray();
                    }
                }

                await _context.Product.AddAsync(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "ProductManagement");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product update, IFormFile image)
        {
            var existingProduct = await _context.Product.FirstOrDefaultAsync(c => c.id_product == update.id_product);

            if (string.IsNullOrWhiteSpace(update.name))
            {
                ViewBag.ErrorMessage = "Tên sản phẩm không được để trống.";
                return View("Update");
            }
            else if (update.price == 0)
            {
                ViewBag.ErrorMessage = "Giá tiền không được để trống.";
                return View("Update");
            }
            else if (update.price < 0)
            {
                ViewBag.ErrorMessage = "Giá tiền không âm.";
                return View("Update");
            }
            else if (update.id_category == 0)
            {
                ViewBag.ErrorMessage = "Bạn phải chọn một loại sản phẩm.";
                return View("Update");
            }
            else if (update.id_brand == 0)
            {
                ViewBag.ErrorMessage = "Bạn phải chọn một nhãn hàng.";
                return View("Update");
            }
            else
            {
                existingProduct.name = update.name;
                existingProduct.information = string.IsNullOrWhiteSpace(update.information) ? "" : update.information;
                existingProduct.id_category = update.id_category;
                existingProduct.id_brand = update.id_brand;
                existingProduct.price = update.price;
                existingProduct.status = update.status;
                existingProduct.modified_date = DateTime.Now;

                if (image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        existingProduct.image = memoryStream.ToArray();
                    }
                }

                _context.Product.Update(existingProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "ProductManagement");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id_product)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var Product = await _context.Product.FirstOrDefaultAsync(c => c.id_product == id_product);
                    if (Product != null)
                    {
                        _context.Product.Remove(Product);
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                    }
                    return RedirectToAction("Index", "ProductManagement");
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
