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
            List<IDictionary<string, string>> ServiceImages = new List<IDictionary<string, string>>();
            foreach (var file in _context.Service)
            {
                // Image meta-data.
                IDictionary<string, string> tmpDict = new Dictionary<string, string>();
                tmpDict.Add("Title", file.Title);
                tmpDict.Add("Description", file.Description);
                tmpDict.Add("Uri", file.Uri);
                ServiceImages.Add(tmpDict);
            }
            List<IDictionary<string, string>> ImageSlideShowImages = new List<IDictionary<string, string>>();
            foreach (var file in _context.ImageSlideShow)
            {
                // Image meta-data.
                IDictionary<string, string> tmpDict = new Dictionary<string, string>();
                tmpDict.Add("ProjectName", file.ProjectName);
                tmpDict.Add("Country", file.City);
                tmpDict.Add("Uri", file.Uri);
                tmpDict.Add("Website", file.Website.ToString());
                ImageSlideShowImages.Add(tmpDict);
            }
            ViewData["ServiceImages"] = ServiceImages;
            ViewData["ImageSlideShowImages"] = ImageSlideShowImages;
            ViewData["Contacts"] = await _context.Contact.ToListAsync();
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