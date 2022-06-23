using Microsoft.AspNetCore.Mvc;
using srk_website.Data;
using srk_website.Services;
using srk_website.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace srk_website.Controllers
{
    /// <summary>
    /// This controller handles uploading and deleting service images to azure container.
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
        [HttpGet(nameof(Upload))]
        public IActionResult Upload()
        {

            return View();
        }

        [System.ComponentModel.Description("Upload image to azure container and store meta data in database.")]
        [HttpPost(nameof(Upload))]
        public async Task<IActionResult> Upload(string title, string description, IFormFile file)
        {
            // Index 0 is description of the data, e.g image.
            // Index 1 is the datatype, e.g jpg... 
            var ContentType = file.ContentType.Split("/");
            if (ContentType[0] != "image")
            {
                return Problem("You can only upload images!");
            }
            if (!_imageFormats.Contains(ContentType[1]))
            {
                return Problem("The image format is not supported.");
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
                if (uri == "0")
                {
                    _logger.LogError("Image not found in azure storage.");
                    ViewBag.IsResponse = true;
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "Image upload was unsuccessful!";
                    return View();
                }
                // Meta data about image.
                ServiceModel newImage = new ServiceModel(fileName, title, description, uri);

                // Stored in the database.
                await _context.Service.AddAsync(newImage);
                await _context.SaveChangesAsync();
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Image was successfully uploaded!";
                return View();
            }

        }
        [HttpGet(nameof(Delete))]
        public IActionResult Delete()
        {
            // Get the uri for all images in the azure container.
            var files = _context.Service;
            // List of image meta-data.
            List<SelectListItem> images = new List<SelectListItem>();
            foreach (var file in files)
            {
                images.Add(new SelectListItem { Value = file.ImageName, Text = file.Title });
            }
            ViewData["images"] = images;
            return View();


        }

        [HttpPost(nameof(Delete))]
        [System.ComponentModel.Description("Delete image in azure container.")]
        public async Task<IActionResult> Delete(string ImageName)
        {
            // Delete image from azure container.
            BlobResponseDto response = await _storage.DeleteAsync(ImageName);
            // Find image in database.
            var image = await _context.Service.FindAsync(ImageName);
            if (image == null)
            {
                // Return an error message to the client
                _logger.LogError("Image not found in database.");
                return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
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
                ViewBag.IsResponse = true;
                ViewBag.Message = "Image was successfully deleted!";
                return View();
            }
        }
    }
}
