﻿using srk_website.Models;
using srk_website.Services;
using Microsoft.AspNetCore.Mvc;
using srk_website.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace srk_website.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class ProjectImageController : Controller
    {
        private readonly IAzureStorage _storage;
        private readonly List<string> _imageFormats;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageSlideShowController> _logger;
        private readonly IGenerateRandomImageName _generator;

        public ProjectImageController(IAzureStorage storage, ApplicationDbContext context, IConfiguration configuration, ILogger<ImageSlideShowController> logger, IGenerateRandomImageName generator)
        {
            _storage = storage;
            _context = context;
            // List of image formats supported, see appsettings.json.
            _imageFormats = configuration.GetSection("Formats:Images").Get<List<string>>();
            _logger = logger;
            _generator = generator;
        }

        public async Task<IActionResult> Index()
        {
            // Returns an empty array if no files are present at the storage container
            return _context.ProjectImage != null ?
                          View(await _context.ProjectImage.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Storage'  is null.");
        }
        
        [HttpGet(nameof(Upload))]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost(nameof(Upload))]
        [System.ComponentModel.Description("Upload image to azure container and store meta data in database.")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([Bind(include: "Id,Name,ProjectName")] ProjectImageModel projectImage, IFormFile file)
        {
            if (file == null) 
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "You have to upload an image!";
                return View(projectImage);
            }

            if (!ModelState.IsValid)
            {
                return View(projectImage);
            }
            // Index 0 is description of the data, e.g image.
            // Index 1 is the datatype, e.g jpg... 
            var ContentType = file.ContentType.Split("/");
            if (ContentType[0] != "image")
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "You can only upload an image!";
                return View();
            }
            if (!_imageFormats.Contains(ContentType[1]))
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                var formats = _imageFormats.ToString();
                ViewBag.Message = $"Formats supported: {formats}";
                return View();
            }

            // Generating fileNames untill a unique is found.
            string fileName = "";
            while (true)
            {
                fileName = await _generator.Generate(ContentType[1], 20);
                if (!_context.ProjectImage.Where(s => s.ImageName == fileName).Any())
                {
                    break;
                }
            }
            
            BlobResponseDto response = await _storage.UploadAsync(file, fileName);

            // Check if we got an error
            if (response.Error == true)
            {
                // We got an error during upload, return an error with details to the client
                return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
            }
            else
            {
                var images = await _storage.ListAsync();
                string uri = "";
                foreach (var image in images)
                {
                    if (image.Name == fileName)
                    {
                        if (image.Uri != null)
                        {
                            uri = image.Uri;
                            break;
                        }
                    }
                }
                if (uri == "")
                {
                    return Problem("Could not find image in azure container!");
                }

                projectImage.ImageName = fileName;
                projectImage.ImageUri = uri;

                // Stored in the database.
                // Try catch here in the future.
                await _context.ProjectImage.AddAsync(projectImage);
                await _context.SaveChangesAsync();

                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Image was successfully uploaded!";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost(nameof(Delete))]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Delete image in azure container and store meta data in database.")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find image in database.
            var image = await _context.ProjectImage
                .Include(s => s.Projects)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (image == null)
            {
                _logger.LogError("Image meta data is not in database.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Image meta data is not in database.");
            }

            if (image.ImageName == null)
            {
                _logger.LogError("Image name is null.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Image name is null.");
            }
            string imageName = image.ImageName;
            
            // Remove image meta data from database.
            if (image.Projects.Any())
            {
                TempData["IsResponse"] = true;
                TempData["IsSuccess"] = false;
                TempData["Message"] = "Image is used in a project, you cannot delete it.";
                return RedirectToAction(nameof(Index));
            }
            _context.ProjectImage.Remove(image);
            await _context.SaveChangesAsync();

            // Delete image from azure container.
            BlobResponseDto response = await _storage.DeleteAsync(imageName);

            // Check if we got an error
            if (response.Error == true)
            {
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
            }
            else
            {
                // File has been successfully deleted
                TempData["IsResponse"] = true;
                TempData["IsSuccess"] = true;
                TempData["Message"] = "Project image was successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}