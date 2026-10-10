using AJACAS.Data;
using AJACAS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AJACAS.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(projects);
        }

        // GET: /Project/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project model)
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

            _context.Projects.Add(model);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Project added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Project/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == id &&
                    p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: /Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Project model)
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

            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == id &&
                    p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            project.ProjectName = model.ProjectName;
            project.Description = model.Description;
            project.Technologies = model.Technologies;
            project.ProjectUrl = model.ProjectUrl;
            project.GitHubUrl = model.GitHubUrl;
            project.StartDate = model.StartDate;
            project.EndDate = model.EndDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Project updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Project/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p =>
                    p.ProjectId == id &&
                    p.UserId == userId);

            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Project deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}