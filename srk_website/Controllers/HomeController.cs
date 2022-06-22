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
            // Get the uri for all images in the azure container.
            var files = await _storage.ListAsync();
            // List of image meta-data.
            List<IDictionary<string, string>> images = new List<IDictionary<string, string>>();
            foreach (var file in files)
            {
                // See if the image is in the database.
                var img = await _context.ImageSlideShow.FindAsync(file.Name);
                // If the image is not a ImageSlideShow then we check the next image.
                if (img == null)
                {
                    continue;
                }
                // Image meta-data.
                IDictionary<string, string> tmpDict = new Dictionary<string, string>();
                tmpDict.Add("ProjectName", img.ProjectName);
                tmpDict.Add("Country", img.City);
                tmpDict.Add("Uri", file.Uri);
                tmpDict.Add("Website", img.Website.ToString());
                images.Add(tmpDict);
            }
            ViewData["Images"] = images;
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