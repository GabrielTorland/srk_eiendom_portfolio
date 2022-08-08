using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using srk_website.Data;
using srk_website.Models;

namespace srk_website.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AboutController> _logger;

        public AboutController(ApplicationDbContext context, ILogger<AboutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: About
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_context.About.Count() != 1)
            {
                _logger.LogError("There is zero or more than one 'About' in the database");
                return Problem("Can only have one 'About' in the database");
            }
            return _context.About != null ?
                         View(await _context.About.FirstOrDefaultAsync()) :
                         Problem("Entity set 'ApplicationDbContext.About'  is null.");
        }

        // GET: About/Edit/5
        [HttpGet(nameof(Edit))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.About == null)
            {
                return NotFound();
            }

            var aboutModel = await _context.About.FindAsync(id);
            
            if (aboutModel == null)
            {
                return NotFound();
            }
            return View(aboutModel);
        }

        // POST: About/Edit/5
        [HttpPost(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(include: "Id,Text")] AboutModel aboutModel)
        {
            if (id != aboutModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aboutModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutModelExists(aboutModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(aboutModel);
        }

        private bool AboutModelExists(int id)
        {
          return (_context.About?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
