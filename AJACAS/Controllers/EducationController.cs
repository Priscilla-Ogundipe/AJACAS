using AJACAS.Data;
using AJACAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AJACAS.Controllers
{
    [Authorize]
    public class EducationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EducationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Education
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var educations = await _context.Educations
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.IsCurrent)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            return View(educations);
        }

        // GET: /Education/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Education/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Education model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;

            _context.Educations.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Education added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Education/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var education = await _context.Educations
                .FirstOrDefaultAsync(e =>
                    e.EducationId == id &&
                    e.UserId == userId);

            if (education == null)
            {
                return NotFound();
            }

            return View(education);
        }

        // POST: /Education/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Education model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var education = await _context.Educations
                .FirstOrDefaultAsync(e =>
                    e.EducationId == id &&
                    e.UserId == userId);

            if (education == null)
            {
                return NotFound();
            }

            education.Institution = model.Institution;
            education.Degree = model.Degree;
            education.FieldOfStudy = model.FieldOfStudy;
            education.StartDate = model.StartDate;
            education.EndDate = model.EndDate;
            education.IsCurrent = model.IsCurrent;
            education.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Education updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Education/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var education = await _context.Educations
                .FirstOrDefaultAsync(e =>
                    e.EducationId == id &&
                    e.UserId == userId);

            if (education == null)
            {
                return NotFound();
            }

            _context.Educations.Remove(education);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Education deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}