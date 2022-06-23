using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace srk_website.Controllers
{

    [Authorize]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
