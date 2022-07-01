using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using srk_website.Data;
using srk_website.Models;

namespace srk_website.Controllers
{
    [Authorize]
    [Route("Admin/[controller]")]
    public class TestimonialController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimonialController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Testimonial
        [HttpGet]
        public async Task<IActionResult> Index()
        {
              return _context.Testimonial != null ? 
                          View(await _context.Testimonial.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Testimonial'  is null.");
        }

        // GET: Testimonial/Details/5
        [HttpGet(nameof(Details))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Testimonial == null)
            {
                return NotFound();
            }

            var testimonialModel = await _context.Testimonial
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testimonialModel == null)
            {
                return NotFound();
            }

            return View(testimonialModel);
        }

        // GET: Testimonial/Create
        [HttpGet(nameof(Create))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Testimonial/Create
        [HttpPost(nameof(Create))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(include: "FirstName,LastName,Project,Position,Testimonial")] TestimonialModel testimonialModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(testimonialModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(testimonialModel);
        }

        // GET: Testimonial/Edit/5
        [HttpGet(nameof(Edit))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Testimonial == null)
            {
                return NotFound();
            }

            var testimonialModel = await _context.Testimonial.FindAsync(id);
            if (testimonialModel == null)
            {
                return NotFound();
            }
            return View(testimonialModel);
        }

        // POST: Testimonial/Edit/5
        [HttpPost(nameof(Edit))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind(include: "Id,FirstName,LastName,Project,Position,Testimonial")] TestimonialModel testimonialModel)
        {
            if (id != testimonialModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testimonialModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestimonialModelExists(testimonialModel.Id))
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
            return View(testimonialModel);
        }

        // GET: Testimonial/Delete/5
        [HttpGet(nameof(Delete))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Testimonial == null)
            {
                return NotFound();
            }

            var testimonialModel = await _context.Testimonial
                .FirstOrDefaultAsync(m => m.Id == id);
            if (testimonialModel == null)
            {
                return NotFound();
            }

            return View(testimonialModel);
        }

        // POST: Testimonial/Delete/5
        [HttpPost(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Testimonial == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Testimonial'  is null.");
            }
            var testimonialModel = await _context.Testimonial.FindAsync(id);
            if (testimonialModel != null)
            {
                _context.Testimonial.Remove(testimonialModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestimonialModelExists(int id)
        {
          return (_context.Testimonial?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
