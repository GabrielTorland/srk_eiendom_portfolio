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
        private readonly IGenerateRandomImageName _generator;

        public ServiceController(IAzureStorage storage, ApplicationDbContext context, IConfiguration configuration, ILogger<ServiceController> logger, IGenerateRandomImageName generator)
        {
            _storage = storage;
            _context = context;
            // List of image formats supported, see appsettings.json.
            _imageFormats = configuration.GetSection("Formats:Images").Get<List<string>>();
            _logger = logger;
            _generator = generator;
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
        [ValidateAntiForgeryToken]
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


            // Generating fileNames untill a unique is found.
            string fileName = "";
            while (true)
            {
                fileName = await _generator.Generate(ContentType[1], 20);
                if (_context.Service.Where(s => s.ImageName == fileName).Count() == 0)
                {
                    break;
                }
            }

            // Upload image to azure container.
            BlobResponseDto? response = await _storage.UploadAsync(file, fileName);
            
            if (response == null)
            {
                return Problem("UploadAsync is not working or azure storage is down!");
            }
            
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
                if (images == null)
                {
                    return Problem("ListAsync is not working or azure storage is down!");
                }
                string uri = "";
                foreach (var image in images)
                {
                    if (image.Name == fileName)
                    {
                        uri = image.Uri;
                        break;
                    }
                }
                // This test is a little overkill, but what the heck:D
                if (uri == "")
                {
                    return Problem("Could not find image in azure container!");
                }
                
                // Meta data about image.
                ServiceModel newImage = new ServiceModel(fileName, title, description, uri);

                // Stored in the database.
                // Try catch here in the future.
                await _context.Service.AddAsync(newImage);
                await _context.SaveChangesAsync();
                
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = true;
                ViewBag.Message = "Service was successfully uploaded!";
                return View();
            }

        }

        [HttpPost(nameof(Delete))]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Delete image in azure container and remove meta data in database.")]
        public async Task<IActionResult> Delete(string ImageName)
        {
            if (ImageName == null)
            {
                return Problem("ImageName cant be null.");
            }
            
            // Find image in database.
            var image = await _context.Service.FindAsync(ImageName);
            if (image == null)
            {
                _logger.LogError("Service not found in database.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Service not in database!");
            }
            // Remove image from database.
            // Try catch here in the future.
            _context.Service.Remove(image);
            await _context.SaveChangesAsync();

            // Delete image from azure container.
            BlobResponseDto response = await _storage.DeleteAsync(ImageName);            

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
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Edit image in azure container and edit meta data in database.")]
        public async Task<IActionResult> Edit(string imageName, string title, string description, IFormFile file)
        {
            if (imageName == null | title == null | description == null)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "All parameters needs to be filled!";
                return View();
            }
            string ImageName = "";
            if (file != null)
            {
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
                // ImageName change because another datatype is used.
                ImageName = imageName.Split('.')[0] + '.' + ContentType[1];
            }
            else
            {
                ImageName = imageName;
            }
            

            var service = await _context.Service.FindAsync(imageName);
            if (service == null)
            {
                return NotFound();
            }

            var newService = new ServiceModel();
            newService.ImageName = ImageName;
            newService.Title = title;
            newService.Description = description;
            // Replace old image name with new imagename(otherwise URI is still the same).
            newService.Uri = service.Uri.Replace(imageName, ImageName);

            // Try catch here in the future.
            _context.Service.Remove(service);
            _context.Service.Add(newService);
            _context.SaveChanges();

            if (file != null)
            {
                // Delete image in azure storage.
                BlobResponseDto? response = await _storage.DeleteAsync(imageName);
                if (response.Error == true)
                {
                    _logger.LogError("Failed to delete image from azure container.");
                    return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                }
                // Upload image to azure container.
                response = await _storage.UploadAsync(file, ImageName);
                if (response.Error == true)
                {
                    _logger.LogError("Failed to upload to azure container.");
                    return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                }
            }

            ViewBag.IsResponse = true;
            ViewBag.IsSuccess = true;
            ViewBag.Message = "Service was successfully edited!";
            return View(newService);
            
        }
    }
}
