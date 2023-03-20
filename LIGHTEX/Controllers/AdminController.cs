using Microsoft.AspNetCore.Mvc;

namespace LIGHTEX.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
