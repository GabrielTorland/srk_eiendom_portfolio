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
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageSlideShowController> _logger;

        public ProjectController(ApplicationDbContext context, ILogger<ImageSlideShowController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Project
        public async Task<IActionResult> Index()
        {
              return _context.Project != null ? 
                          View(await _context.Project.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Project'  is null.");
        }

        // GET: Project/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var projectModel = await _context.Project
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectModel == null)
            {
                return NotFound();
            }

            return View(projectModel);
        }

        // GET: Project/Create
        [HttpGet]
        public IActionResult Create()
        {
            var images = _context.Storage.Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.ImageUri,
                                      Text = a.Name
                                  }).ToList();
            if (images.Count == 0)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "<p> No images found in the remote storage. You have to <a style='font-weight:bold' href='/Admin/Storage/Upload'> upload </a> an image before you can create a project. </p>";
            }
            else
            {
                ViewBag.Images = new MultiSelectList(images, "Value", "Text");
            }
            
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(include: "Id,Title,ProjectDescription,CoverImageId")] ProjectModel projectModel, string[] selectedImages)
        {
            var images = _context.Storage.Select(a =>
                                  new SelectListItem
                                  {
                                      Value = a.ImageUri,
                                      Text = a.Name
                                  }).ToList();
            
            if (selectedImages.Length <= 0)
            {
                ViewBag.Images = new MultiSelectList(images, "Value", "Text");
                ViewBag.NoImages = "You have to select at least one image.";
                return View(projectModel);
            }
            if (ModelState.IsValid)
            {
                projectModel.Images = _context.Storage.Where(a => selectedImages.Contains(a.ImageUri)).ToList();
                
                if (projectModel.Images.Count != selectedImages.Length)
                {
                    ViewBag.Images = new MultiSelectList(images, "Value", "Text");
                    ViewBag.IsResponse = true;
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "<p> Someone deleted the image while you were creating the project. You have to <a style='font-weight:bold' href='/Admin/Storage/Upload'> upload </a> the image again. </p>";
                    return View(projectModel);
                }
                else
                {
                    _context.Add(projectModel);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            
            ViewBag.Images = new MultiSelectList(images, "Value", "Text");
            ViewBag.SelectedImages = selectedImages;
            return View(projectModel);
        }

        // GET: Project/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var projectModel = await _context.Project.FindAsync(id);
            if (projectModel == null)
            {
                return NotFound();
            }
            return View(projectModel);
        }

        // POST: Project/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ProjectDescription")] ProjectModel projectModel)
        {
            if (id != projectModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectModelExists(projectModel.Id))
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
            return View(projectModel);
        }

        // GET: Project/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var projectModel = await _context.Project
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectModel == null)
            {
                return NotFound();
            }

            return View(projectModel);
        }

        // POST: Project/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Project'  is null.");
            }
            var projectModel = await _context.Project.FindAsync(id);
            if (projectModel != null)
            {
                _context.Project.Remove(projectModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectModelExists(int id)
        {
          return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
