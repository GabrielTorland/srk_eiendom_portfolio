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

        [Authorize]
        [Route("Admin/Project")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
              return _context.Project != null ? 
                          View(await _context.Project.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Project'  is null.");
        }

        [Authorize]
        [Route("Admin/Project/Details")]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [Authorize]
        [Route("Prosjekt")]
        [HttpGet]
        public async Task<IActionResult> Prosjekt(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [Authorize]
        [Route("Admin/Project/Create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var oImages = await _context.ProjectImage.OrderBy(pi => pi.ProjectName).ToListAsync();
            if (oImages.Count == 0)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "<p> No images found in the remote storage. You have to <a style='font-weight:bold' href='/Admin/ProjectImage/Upload'> upload </a> an image before you can create a project. </p>";
            }
            else
            {
                if (oImages.First().ProjectName == null)
                {
                    return Problem("ProjectName is null!");
                }
                string? currentGroup = oImages.First().ProjectName;

                if (currentGroup == null)
                {
                    return Problem("Something went wrong");
                }

                var images = ProjectImagesDict(oImages, currentGroup);

                ViewBag.Images = images;
            }
            
            return View();
        }

        [Authorize]
        [Route("Admin/Project/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(include: "Title,ProjectDescription,Location,CoverImageUri,ThumbnailUri")] ProjectModel project, string[] selectedImages)
        {

            if (!selectedImages.Contains(project.CoverImageUri))
            {
                return Problem("The cover image has to be one of the selected images");
            }

            if (!selectedImages.Contains(project.ThumbnailUri))
            {
                return Problem("The thumbnail image has to be one of the selected images");
            }
            
            var oImages = await _context.ProjectImage.OrderBy(pi => pi.ProjectName).ToListAsync();

            if (oImages == null)
            {
                return Problem("oImages is null");
            }

            if (oImages.First().ProjectName == null)
            {
                return Problem("ProjectName is null!");
            }
            string? currentGroup = oImages.First().ProjectName;

            if (currentGroup == null)
            {
                return Problem("Something went wrong");
            }

            var images = ProjectImagesDict(oImages, currentGroup);
            ViewBag.Images = images;

            var sImages = _context.ProjectImage.Where(a => selectedImages.Contains(a.ImageUri)).ToList();
            var selected = new Dictionary<string, string>();
            foreach (var image in sImages)
            {
                if (image.Name == null || image.ImageUri == null)
                {
                    return Problem("Name or Uri is null!");
                }
                selected[image.ImageUri] = image.Name;
            }
            ViewBag.SelectedImages = selected;

            if (selectedImages.Length != 4)
            {
                ViewBag.selectedProjectImagesError = "You have to select 4 images.";
                return View(project);
            }

            if (ModelState.IsValid)
            {
                project.Images = _context.ProjectImage.Where(a => selectedImages.Contains(a.ImageUri)).ToList();
                
                if (project.Images.Count != selectedImages.Length)
                {
                    return Problem("A selected image got deleted while you were creating the project. Try again!");
                }
                else
                {
                    _context.Add(project);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(project);
        }

        [Authorize]
        [Route("Admin/Project/Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var oImages = await _context.ProjectImage.OrderBy(pi => pi.ProjectName).ToListAsync();

            if (oImages == null)
            {
                return Problem("oImages is null");
            }

            if (oImages.First().ProjectName == null)
            {
                return Problem("ProjectName is null!");
            }
            string? currentGroup = oImages.First().ProjectName;

            if (currentGroup == null)
            {
                return Problem("Something went wrong");
            }

            var images = ProjectImagesDict(oImages, currentGroup);
            ViewBag.Images = images;

            var project = await _context.Project
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            IDictionary<string, string> selected = new Dictionary<string, string>();
            foreach (var image in project.Images)
            {
                if (image.Name == null || image.ImageUri == null)
                {
                    return Problem("Name or Uri is null!");
                }
                selected[image.ImageUri] = image.Name;
            }
            ViewBag.SelectedImages = selected;

            return View(project);
        }

        [Authorize]
        [Route("Admin/Project/Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(include: "Id,Title,ProjectDescription,Location,CoverImageUri,ThumbnailUri")] ProjectModel project, string[] selectedImages)
        {

            if (id != project.Id)
            {
                return NotFound();
            }

            if (!selectedImages.Contains(project.CoverImageUri))
            {
                return Problem("The cover image has to be one of the selected images");
            }
            if (!selectedImages.Contains(project.ThumbnailUri))
            {
                return Problem("The thumbnail image has to be one of the selected images");
            }

            var oImages = await _context.ProjectImage.OrderBy(pi => pi.ProjectName).ToListAsync();

            if (oImages == null)
            {
                return Problem("oImages is null");
            }

            if (oImages.First().ProjectName == null)
            {
                return Problem("ProjectName is null!");
            }
            string? currentGroup = oImages.First().ProjectName;

            if (currentGroup == null)
            {
                return Problem("Something went wrong");
            }

            var images = ProjectImagesDict(oImages, currentGroup);
            ViewBag.Images = images;

            var sImages = _context.ProjectImage.Where(a => selectedImages.Contains(a.ImageUri)).ToList();
            var selected = new Dictionary<string, string>();
            foreach (var image in sImages)
            {
                if (image.Name == null || image.ImageUri == null)
                {
                    return Problem("Name or Uri is null!");
                }
                selected[image.ImageUri] = image.Name;
            }
            ViewBag.SelectedImages = selected;

            if (selectedImages.Length != 4)
            {
                ViewBag.selectedProjectImagesError = "You have to select 4 images.";
                return View(project);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectModelExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Project successfully edited.";
                return View(project);
            }

            return View(project);
        }

        [Authorize]
        [Route("Admin/Project/Delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [Authorize]
        [Route("Admin/Project/Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Project'  is null.");
            }
            var project = await _context.Project.FindAsync(id);
            if (project != null)
            {
                _context.Project.Remove(project);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectModelExists(int id)
        {
          return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Convert a sorted list of ProjectImageModel to a dictionary(key: ProjectName, value: Multiselect)
        private IDictionary<string, MultiSelectList>? ProjectImagesDict(List<ProjectImageModel> oImages, string? currentGroup)
        {
            IDictionary<string, MultiSelectList> images = new Dictionary<string, MultiSelectList>();
            var items = new List<SelectListItem>();
            foreach (var item in oImages)
            {
                if (item.ProjectName != currentGroup)
                {
                    if (items.Count != 0)
                    {
                        if (currentGroup == null)
                        {
                            _logger.LogError("currentGroup is null in ProjectImageDict function");
                            return null;
                        }
                        images[currentGroup] = new MultiSelectList(items, "Value", "Text");
                        items = new List<SelectListItem>();
                    }
                    currentGroup = item.ProjectName;
                }
                items.Add(new SelectListItem { Value = item.ImageUri, Text = item.Name });
            }
            if (currentGroup == null)
            {
                _logger.LogError("currentGroup is null in ProjectImageDict function");
                return null;
            }
            images[currentGroup] = new MultiSelectList(items, "Value", "Text");
            return images;

        }
    }
}
