using srk_website.Models;
using srk_website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using srk_website.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace srk_website.Controllers
{
    /// <summary>
    /// This controller handles uploading and deleting slide show images to azure container.
    /// </summary>
    /// <remarks></remarks>
    [Route("api/[controller]")]
    [Authorize]
    public class ImageSlideShowController : Controller
    {
        private readonly IAzureStorage _storage;
        private readonly List<string> _imageFormats;
        private readonly ApplicationDbContext _context;

        public ImageSlideShowController(IAzureStorage storage, ApplicationDbContext context, IConfiguration configuration)
        {
            _storage = storage;
            _context = context;
            // List of image formats supported, see appsettings.json.
            _imageFormats = configuration.GetSection("Formats:Images").Get<List<string>>();
        }
        [HttpGet(nameof(Upload))]
        public IActionResult Upload()
        {
            return View();
        }

        [System.ComponentModel.Description("Upload image to azure container and store meta data in database.")]
        [HttpPost(nameof(Upload))]
        public async Task<IActionResult> Upload(string projectName, string city, string website, IFormFile file)
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

            // Meta data about image.
            ImageSlideShowModel newImage = new ImageSlideShowModel(file.FileName, projectName, city, website);
            // Stored in the database.
            await _context.ImageSlideShow.AddAsync(newImage);
            await _context.SaveChangesAsync();

            // Upload image to azure container.
            BlobResponseDto? response = await _storage.UploadAsync(file, file.FileName);

            // Check if we got an error
            if (response.Error == true)
            {
                // We got an error during upload, return an error with details to the client
                return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
            }
            else
            {
                // Return a success message to the client about successfull upload
                return StatusCode(StatusCodes.Status200OK, response);
            }

        }
        [HttpGet(nameof(Delete))]
        public async Task<IActionResult> Delete()
        {
            // Get the uri for all images in the azure container.
            var files = await _storage.ListAsync();
            // List of image meta-data.
            List<SelectListItem> images = new List<SelectListItem>();
            foreach (var file in files)
            {
                // See if the image is in the database.
                var img = await _context.ImageSlideShow.FindAsync(file.Name);
                // If the image is not a ImageSlideShow then we check the next image.
                if (img == null)
                {
                    continue;
                }
                images.Add(new SelectListItem { Value = img.ImageName, Text = img.ProjectName });
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
            var image = await _context.ImageSlideShow.FindAsync(ImageName);
            if (image == null)
            {
                return Problem("Image is not in the database.");
            }
            // Remove image from database.
            _context.ImageSlideShow.Remove(image);
            await _context.SaveChangesAsync();

            // Check if we got an error
            if (response.Error == true)
            {
                // Return an error message to the client
                return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
            }
            else
            {
                // File has been successfully deleted
                return StatusCode(StatusCodes.Status200OK, response.Status);
            }
        }        
    }
}
