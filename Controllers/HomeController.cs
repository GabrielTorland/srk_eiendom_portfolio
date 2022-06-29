using Microsoft.AspNetCore.Mvc;
using srk_website.Data;
using srk_website.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using srk_website.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace srk_website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorage _storage;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IAzureStorage storage)
        {
            _storage = storage;
            _context = context;
            _logger = logger;
        }
        
        public async Task<IActionResult> Index()
        {
            ViewData["Experience"] = DateTime.Now.Year - 2018; // The firm was founded in 2018
            ViewData["About"] = await _context.About.FindAsync(1);
            ViewData["Services"] = await _context.Service.ToListAsync();
            ViewData["ImageSlideShows"] = await _context.ImageSlideShow.ToListAsync();
            ViewData["Contacts"] = await _context.Contact.ToListAsync();
            ViewData["TeamMembers"] = await _context.Team.ToListAsync();
            return View();
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}