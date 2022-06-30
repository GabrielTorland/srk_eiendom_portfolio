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
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: About
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // We only have one about page(i.e. only id equal 1).
            var about = await _context.About.FindAsync(1);
            if (about == null)
            {
                // About page was not created in db initilizer.
                return StatusCode(StatusCodes.Status500InternalServerError, "About doesnt exist");
            }
            return View(about);
 
        }

        // GET: About/Edit/5
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
        [HttpPost]
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
