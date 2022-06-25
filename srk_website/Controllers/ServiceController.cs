using Microsoft.AspNetCore.Mvc;
using srk_website.Data;
using srk_website.Services;
using srk_website.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace srk_website.Controllers
{
    /// <summary>
    /// This controller handles upload, delete and edit for services.
    /// </summary>
    /// <remarks></remarks>
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceController : Controller
    {
        private readonly IAzureStorage _storage;
        private readonly List<string> _imageFormats;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(IAzureStorage storage, ApplicationDbContext context, IConfiguration configuration, ILogger<ServiceController> logger)
        {
            _storage = storage;
            _context = context;
            // List of image formats supported, see appsettings.json.
            _imageFormats = configuration.GetSection("Formats:Images").Get<List<string>>();
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View(_context.Service);
        }
            
        [HttpGet(nameof(Upload))]
        public IActionResult Upload()
        {

            return View();
        }

        [System.ComponentModel.Description("Upload image to azure container and store meta data in database.")]
        [HttpPost(nameof(Upload))]
        public async Task<IActionResult> Upload(string title, string description, IFormFile file)
        {
            if (title == null | description == null | file == null)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "All parameters needs to be filled!";
                return View();
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

            string fileName = $"{_context.Service.Count() + 1}" + '.' + ContentType[1];

            // Upload image to azure container.
            BlobResponseDto? response = await _storage.UploadAsync(file, fileName);
            
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
                string uri = "0";
                foreach (var image in images)
                {
                    if (image.Name == fileName)
                    {
                        uri = image.Uri;
                    }
                }
                // Meta data about image.
                ServiceModel newImage = new ServiceModel(fileName, title, description, uri);

                // Stored in the database.
                await _context.Service.AddAsync(newImage);
                await _context.SaveChangesAsync();
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Service was successfully uploaded!";
                return View();
            }

        }

        [HttpPost(nameof(Delete))]
        [System.ComponentModel.Description("Delete image in azure container and remove meta data in database.")]
        public async Task<IActionResult> Delete(string ImageName)
        {
            if (ImageName == null)
            {
                return Problem("ImageName cant be null.");
            }
            // Delete image from azure container.
            BlobResponseDto response = await _storage.DeleteAsync(ImageName);
            // Find image in database.
            var image = await _context.Service.FindAsync(ImageName);
            if (image == null)
            {
                _logger.LogError("Service not found in database.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Service not in database!");
            }
            // Remove image from database.
            _context.Service.Remove(image);
            await _context.SaveChangesAsync();

            // Check if we got an error
            if (response.Error == true)
            {
                // Return an error message to the client
                _logger.LogError("Failed to delete image from azure container.");
                return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
            }
            else
            {
                // File has been successfully deleted
                return RedirectToAction("Index", "Service");
            }
        }

        [HttpGet(nameof(Edit))]
        public async Task<IActionResult> Edit(string ImageName)
        {
            if (ImageName == null)
            {
                return NotFound();
            }
            var service = await _context.Service.FindAsync(ImageName);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        
        // This method needs to be improved in the future!
        [HttpPost(nameof(Edit))]
        public async Task<IActionResult> Edit(string imageName, string title, string description, IFormFile file)
        {
            if (imageName == null | title == null | description == null | file == null)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "All parameters needs to be filled!";
                return View();
            }
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
            var service = await _context.Service.FindAsync(imageName);
            if (service == null)
            {
                return NotFound();
            }
            // Have to create new image name because datatype can change.
            string ImageName = imageName.Substring(0, 1) + '.' + ContentType[1];

            var newService = new ServiceModel();
            newService.ImageName = ImageName;
            newService.Title = title;
            newService.Description = description;
            newService.Uri = service.Uri.Replace(imageName, ImageName);

            _context.Service.Remove(service);
            _context.Service.Add(newService);
            _context.SaveChanges();

            // Delete image in azure storage.
            await _storage.DeleteAsync(imageName);
            
            // Upload image to azure container.
            await _storage.UploadAsync(file, ImageName);
            ViewBag.IsResponse = true;
            ViewBag.IsSuccess = true;
            ViewBag.Message = "Service was successfully edited!";
            return View(newService);
            
        }
    }
}
