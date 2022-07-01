using Microsoft.AspNetCore.Mvc;
using srk_website.Data;
using srk_website.Services;
using srk_website.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace srk_website.Controllers
{
    /// <summary>
    /// This controller handles upload, delete and edit for services.
    /// </summary>
    /// <remarks></remarks>
    [Authorize]
    [Route("Admin/[controller]")]
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
        [HttpGet]
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
        public async Task<IActionResult> Upload([Bind(include: "Title, Description")] ServiceModel service, IFormFile file)
        {   
            if (file == null)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "You have to upload an image!";
                return View(service);
            }

            if (!ModelState.IsValid)
            {
                return View(service);
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
                if (!_context.Service.Where(s => s.ImageName == fileName).Any())
                {
                    break;
                }
            }

            // Upload image to azure container.
            BlobResponseDto response = await _storage.UploadAsync(file, fileName);
            
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
                        if (image.Uri != null)
                        {
                            uri = image.Uri;
                            break;
                        }
                    }
                }
                // This test is a little overkill, but what the heck:D
                if (uri == "")
                {
                    return Problem("Could not find image in azure container!");
                }

                // Update parameters
                service.ImageName = fileName;
                service.Uri = uri;

                // Stored in the database.
                // Try catch here in the future.
                await _context.Service.AddAsync(service);
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find image in database.
            var image = await _context.Service.FindAsync(id);
            
            if (image == null)
            {
                _logger.LogError("Service not found in database.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Service not in database!");
            }

            if (image.ImageName == null)
            {
                _logger.LogError("Image name is null.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Image name is null!");
            }

            string imageName = image.ImageName;
            
            // Remove image from database.
            // Try catch here in the future.
            _context.Service.Remove(image);
            await _context.SaveChangesAsync();
            
            // Delete image from azure container.
            BlobResponseDto response = await _storage.DeleteAsync(imageName);            

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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var service = await _context.Service.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind(include: "Id,Title,Description,ImageName,Uri")] ServiceModel service, IFormFile file)
        {
            if (id != service.Id)
            {
                return NotFound();
            }
            
            ModelState.Remove("file");
            if (!ModelState.IsValid)
            {
                return View(service);
            }
            
            if (service.ImageName == null)
            {
                _logger.LogError("Image name is null.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Image name is null!");
            }
            string imageName;
            if (file != null)
            {
                var ContentType = file.ContentType.Split("/");
                if (ContentType[0] != "image")
                {
                    ViewBag.IsResponse = true;
                    ViewBag.IsSuccess = false;
                    ViewBag.Message = "You can only upload an image!";
                    return View(service);
                }
                if (!_imageFormats.Contains(ContentType[1]))
                {
                    ViewBag.IsResponse = true;
                    ViewBag.IsSuccess = false;
                    var formats = _imageFormats.ToString();
                    ViewBag.Message = $"Formats supported: {formats}";
                    return View(service);
                }
                // ImageName change because another datatype is used.
                imageName = service.ImageName.Split('.')[0] + '.' + ContentType[1];
            }
            else
            {
                imageName = service.ImageName;
            }

            // Replace old image name with new imagename(otherwise URI is still the same).
            service.ImageName = imageName;
            if (service.Uri == null)
            {
                _logger.LogError("Uri is null.");
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, "Uri is null!");
            }
            service.Uri = service.Uri.Replace(service.ImageName, imageName);

            try
            {
                _context.Service.Update(service);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceModelExists(service.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (file != null)
            {
                // Delete image in azure storage.
                BlobResponseDto response = await _storage.DeleteAsync(imageName);
                if (response.Error == true)
                {
                    _logger.LogError("Failed to delete image from azure container.");
                    // Delete meta data from database if image was deleted unsuccessfully from azure storage.
                    _context.Service.Remove(service);
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                }
                // Upload image to azure container.
                response = await _storage.UploadAsync(file, imageName);
                if (response.Error == true)
                {
                    _logger.LogError("Failed to upload to azure container.");
                    // Delete meta data from database if image was uploaded unsuccessfully to azure storage.
                    _context.Service.Remove(service);
                    await _context.SaveChangesAsync();
                    return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                }
            }

            ViewBag.IsResponse = true;
            ViewBag.IsSuccess = true;
            ViewBag.Message = "Service was successfully edited!";
            return View(service);
            
        }
        private bool ServiceModelExists(int id)
        {
            return (_context.Service?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
