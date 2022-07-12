using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using srk_website.Data;
using srk_website.Models;
using srk_website.Services;

namespace srk_website.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class TeamMemberController : Controller
    {
        private readonly IAzureStorage _storage;
        private readonly List<string> _imageFormats;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImageSlideShowController> _logger;
        private readonly IGenerateRandomImageName _generator;

        public TeamMemberController(IAzureStorage storage, ApplicationDbContext context, IConfiguration configuration, ILogger<ImageSlideShowController> logger, IGenerateRandomImageName generator)
        {
            _storage = storage;
            _context = context;
            // List of image formats supported, see appsettings.json.
            _imageFormats = configuration.GetSection("Formats:Images").Get<List<string>>();
            _logger = logger;
            _generator = generator;
        }

        // GET: Team
        [HttpGet]
        public async Task<IActionResult> Index()
        {
              return _context.TeamMember != null ? 
                          View(await _context.TeamMember.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Team'  is null.");
        }

        // GET: Team/Details/5
        [HttpGet(nameof(Details))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TeamMember == null)
            {
                return NotFound();
            }

            var teamModel = await _context.TeamMember
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teamModel == null)
            {
                return NotFound();
            }

            return View(teamModel);
        }

        // GET: Team/Create
        [HttpGet(nameof(Create))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Team/Create
        [HttpPost(nameof(Create))]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Upload image to azure container and store meta data in database.")]
        public async Task<IActionResult> Create([Bind(include: "FirstName,LastName,Position,Email,Phone,LinkedIn")] TeamMemberModel teamMember, IFormFile file)
        {
            if (file == null)
            {
                ViewBag.IsResponse = true;
                ViewBag.IsSuccess = false;
                ViewBag.Message = "You have to upload an image!";
                return View();
            }
            
            if (ModelState.IsValid)
            {
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
                    if (_context.TeamMember.Where(s => s.ImageName == fileName).Count() == 0)
                    {
                        break;
                    }
                }

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
                    teamMember.ImageName = fileName;
                    teamMember.Uri = uri;
                    // Try catch here in the future.
                    await _context.TeamMember.AddAsync(teamMember);
                    await _context.SaveChangesAsync();
                    ViewBag.IsResponse = true;
                    ViewBag.IsSuccess = true;
                    ViewBag.Message = "Image was successfully uploaded to the slideshow!";
                    
                    return View();
                }
            }
            return View(teamMember);
        }

        // GET: Team/Edit/5
        [HttpGet(nameof(Edit))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TeamMember == null)
            {
                return NotFound();
            }

            var teamModel = await _context.TeamMember.FindAsync(id);
            if (teamModel == null)
            {
                return NotFound();
            }
            return View(teamModel);
        }

        // POST: Team/Edit/5
        [HttpPost(nameof(Edit))]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Edit image in azure container and edit meta data in database.")]
        public async Task<IActionResult> Edit(int id, [Bind(include: "Id,FirstName,LastName,Position,Email,Phone,LinkedIn,ImageName,Uri")] TeamMemberModel teamMember, IFormFile file)
        {
            if (id != teamMember.Id)
            {
                return NotFound();
            }

            // Ignore if file is null, because then the user did not change the image.
            ModelState.Remove("file");

            if (ModelState.IsValid)
            {
                if (file != null) {
                    
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

                    if (teamMember.ImageName == null)
                    {
                        return Problem("ImageName is null");
                    }

                    // Delete old image from azure container
                    BlobResponseDto response = await _storage.DeleteAsync(teamMember.ImageName);
                    if (response.Error == true)
                    {
                        // Remove meta data from database.
                        _context.TeamMember.Remove(teamMember);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogError("Failed to delete image from azure container.");
                        return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                    }
                    else
                    {
                        // Upload new image to azure container.
                        string imageName = teamMember.ImageName.Split('.')[0] + '.' + ContentType[1];
                        response = await _storage.UploadAsync(file, imageName);
                        if (response.Error == true)
                        {
                            // Remove meta data from database.
                            _context.TeamMember.Remove(teamMember);
                            await _context.SaveChangesAsync();
                            
                            _logger.LogError("Failed to new upload image to azure container.");
                            return StatusCode(StatusCodes.Status500InternalServerError, response.Status);
                        }
                        else
                        {
                            if (teamMember.Uri == null) 
                            {
                                _logger.LogError("Uri is null!");
                                return StatusCode(StatusCodes.Status500InternalServerError, "Uri is null!");
                            }
                            // Update the team member
                            teamMember.Uri = teamMember.Uri.Replace(teamMember.ImageName, imageName);
                            teamMember.ImageName = imageName;
                        }
                    }
                }
                try
                {
                    _context.Update(teamMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamModelExists(teamMember.Id))
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
                ViewBag.Message = "Team member was successfully edited!";
                return View(teamMember);
            }
            ViewBag.IsResponse = true;
            ViewBag.IsSuccess = false;
            ViewBag.Message = "Failed to edit team member!";
            return View(teamMember);
        }

        // POST: Team/Delete/5
        [HttpPost(nameof(Delete))]
        [ValidateAntiForgeryToken]
        [System.ComponentModel.Description("Delete image in azure container and meta data in database.")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TeamMember == null)
            {
                return NotFound();
            }
            
            var teamMember = await _context.TeamMember.FindAsync(id);
            if (teamMember == null)
            {
                return NotFound();
            }
            
            if (teamMember.ImageName == null)
            {
                _logger.LogError("ImageName is null!");
                return StatusCode(StatusCodes.Status500InternalServerError, "ImageName is null!");
            }
            
            string imageName = teamMember.ImageName;

            // Delete meta data from database
            // Try catch here in the future.
            _context.TeamMember.Remove(teamMember);
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
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TeamModelExists(int id)
        {
          return (_context.TeamMember?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
