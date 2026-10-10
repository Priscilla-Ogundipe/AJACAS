using AJACAS.Data;
using AJACAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AJACAS.Controllers
{
    [Authorize]
    public class WorkExperienceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkExperienceController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /WorkExperience
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var experiences = await _context.WorkExperiences
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.IsCurrent)
                .ThenByDescending(w => w.StartDate)
                .ToListAsync();

            return View(experiences);
        }

        // GET: /WorkExperience/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /WorkExperience/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkExperience model)
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

            _context.WorkExperiences.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Work experience added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /WorkExperience/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var experience = await _context.WorkExperiences
                .FirstOrDefaultAsync(w =>
                    w.WorkExperienceId == id &&
                    w.UserId == userId);

            if (experience == null)
            {
                return NotFound();
            }

            return View(experience);
        }

        // POST: /WorkExperience/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            WorkExperience model)
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

            var experience = await _context.WorkExperiences
                .FirstOrDefaultAsync(w =>
                    w.WorkExperienceId == id &&
                    w.UserId == userId);

            if (experience == null)
            {
                return NotFound();
            }

            experience.CompanyName = model.CompanyName;
            experience.JobTitle = model.JobTitle;
            experience.Location = model.Location;
            experience.StartDate = model.StartDate;
            experience.EndDate = model.EndDate;
            experience.IsCurrent = model.IsCurrent;
            experience.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Work experience updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /WorkExperience/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var experience = await _context.WorkExperiences
                .FirstOrDefaultAsync(w =>
                    w.WorkExperienceId == id &&
                    w.UserId == userId);

            if (experience == null)
            {
                return NotFound();
            }

            _context.WorkExperiences.Remove(experience);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Work experience deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}