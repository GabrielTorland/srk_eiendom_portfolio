using srk_website.Models;
using srk_website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using srk_website.Data;

namespace srk_website.Controllers
{
    /// <summary>
    /// This controller handles delete and upload for slideshow images.
    /// </summary>
    /// <remarks></remarks>
    [Authorize]
    [Route("Admin/[controller]")]
    public class ImageSlideShowController : Controller
    {
        private readonly IAzureStorage _storage;
        private readonly List<string> _imageFormats;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageSlideShowController> _logger;
        private readonly IGenerateRandomImageName _generator;

        public ImageSlideShowController(IAzureStorage storage, ApplicationDbContext context, IConfiguration configuration, ILogger<ImageSlideShowController> logger, IGenerateRandomImageName generator)
        {
            _storage = storage;
            _context = context;
            // List of image formats supported, see appsettings.json.
            _imageFormats = configuration.GetSection("Formats:Images").Get<List<string>>();
            _logger = logger;
            _generator = generator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_context.ImageSlideShow);
        }
        
        [HttpGet(nameof(Upload))]
        public IActionResult Upload()
        {
            return View();
        }

        [System.ComponentModel.Description("Upload image to azure container and store meta data in database.")]
        [ValidateAntiForgeryToken]
        [HttpPost(nameof(Upload))]
        public async Task<IActionResult> Upload([Bind(include: "ProjectName,City,Website")] ImageSlideShowModel imageSlideShow, IFormFile file)
        {
            if (file == null)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "You have to upload an image!";
                return View();
            }
            if (!ModelState.IsValid)
            {
                return View(imageSlideShow);
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
            string fileName;
            while (true)
            {
                fileName = await _generator.Generate(ContentType[1], 20);
                if (!_context.ImageSlideShow.Where(s => s.ImageName == fileName).Any())
                {
                    break;
                }
            }

            // Upload image to azure container.
            BlobResponseDto response = await _storage.UploadAsync(file, fileName);

            // Check if we got an error
            if (response.Error == true)
            {
                // We got an error during upload, return an error with details to the client
                _logger.LogError("Failed to upload to azure container.");
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

                imageSlideShow.Uri = uri;
                imageSlideShow.ImageName = fileName;

                // Store in the database.
                // Try catch here in the future.
                await _context.ImageSlideShow.AddAsync(imageSlideShow);
                await _context.SaveChangesAsync();
                
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Image was successfully uploaded to the slideshow!";
                return View();
            }

        }

        [HttpPost(nameof(Delete))]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Delete image in azure container and meta data in database.")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Find image in database.
            var image = await _context.ImageSlideShow.FindAsync(id);       
            if (image == null)
            {
                _logger.LogError("ImageSlideShow not in database.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "ImageSlideShow not in database.");
            }
            
            if (image.ImageName == null)
            {
                _logger.LogError("ImageSlideShow has no image name.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "ImageSlideShow has no image name.");
            }
            string imageName = image.ImageName;

            // Remove image from database.
            // Try catch here in the future.
            _context.ImageSlideShow.Remove(image);
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
                return RedirectToAction("Index", "ImageSlideShow");
            }
        }        
    }
}
