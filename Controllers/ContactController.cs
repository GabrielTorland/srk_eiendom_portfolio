using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using srk_website.Data;
using srk_website.Models;

namespace srk_website.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ApplicationDbContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Contact
        public async Task<IActionResult> Index()
        {   
            if (_context.Contact.Count() != 1)
            {
                _logger.LogError("There is zero or more than one 'Contact' in the database");
                return Problem("Can only have one 'Contact' in the database");
            }
            
            return _context.Contact != null ? 
                          View(await _context.Contact.FirstOrDefaultAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Contact'  is null.");
        }

        [HttpGet(nameof(Edit))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contact == null)
            {
                return NotFound();
            }

            var contactModel = await _context.Contact.FindAsync(id);
            if (contactModel == null)
            {
                return NotFound();
            }
            return View(contactModel);
        }

        [HttpPost(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(include: "Id,Address,Zip,City,Country,Phone,Email")] ContactModel contactModel)
        {
            if (id != contactModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contactModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactModelExists(contactModel.Id))
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
            return View(contactModel);
        }

        private bool ContactModelExists(int id)
        {
          return (_context.Contact?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
