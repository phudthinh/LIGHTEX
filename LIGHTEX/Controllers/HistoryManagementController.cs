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
    public class HistoryManagementController : Controller
    {
        private readonly LIGHTEXContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HistoryManagementController(LIGHTEXContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
