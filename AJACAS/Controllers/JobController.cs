using AJACAS.Services;
using AJACAS.Data;
using AJACAS.Models;
using AJACAS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AJACAS.Controllers
{
    [Authorize]
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JobMatchService _jobMatchService;

        public JobController(
     ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     JobMatchService jobMatchService)
        {
            _context = context;
            _userManager = userManager;
            _jobMatchService = jobMatchService;
        }

        // GET: /Job
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var jobs = await _context.JobDescriptions
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            return View(jobs);
        }

        // GET: /Job/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Job/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            JobDescriptionViewModel model)
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

            var job = new JobDescription
            {
                UserId = userId,
                JobTitle = model.JobTitle.Trim(),
                CompanyName = model.CompanyName?.Trim(),
                Location = model.Location?.Trim(),
                Description = model.Description.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.JobDescriptions.Add(job);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Job description added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Job/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var job = await _context.JobDescriptions
                .Include(j => j.JobMatches)
                .FirstOrDefaultAsync(j =>
                    j.JobDescriptionId == id &&
                    j.UserId == userId);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: /Job/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var job = await _context.JobDescriptions
                .FirstOrDefaultAsync(j =>
                    j.JobDescriptionId == id &&
                    j.UserId == userId);

            if (job == null)
            {
                return NotFound();
            }

            var model = new JobDescriptionViewModel
            {
                JobTitle = job.JobTitle,
                CompanyName = job.CompanyName,
                Location = job.Location,
                Description = job.Description
            };

            return View(model);
        }

        // POST: /Job/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            JobDescriptionViewModel model)
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

            var job = await _context.JobDescriptions
                .FirstOrDefaultAsync(j =>
                    j.JobDescriptionId == id &&
                    j.UserId == userId);

            if (job == null)
            {
                return NotFound();
            }

            job.JobTitle = model.JobTitle.Trim();
            job.CompanyName = model.CompanyName?.Trim();
            job.Location = model.Location?.Trim();
            job.Description = model.Description.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Job description updated successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Job/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var job = await _context.JobDescriptions
                .FirstOrDefaultAsync(j =>
                    j.JobDescriptionId == id &&
                    j.UserId == userId);

            if (job == null)
            {
                return NotFound();
            }

            _context.JobDescriptions.Remove(job);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Job description deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
        // GET: /Job/Match/5
        [HttpGet]
        public async Task<IActionResult> Match(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Make sure the job belongs to the logged-in user
            var job = await _context.JobDescriptions
                .FirstOrDefaultAsync(j =>
                    j.JobDescriptionId == id &&
                    j.UserId == userId);

            if (job == null)
            {
                return NotFound();
            }

            // Get the user's uploaded CVs
            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            if (!resumes.Any())
            {
                TempData["ErrorMessage"] =
                    "You need to upload a CV before you can match it with a job.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            ViewBag.Job = job;

            return View(resumes);
        }
        // POST: /Job/RunMatch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunMatch(
            int jobDescriptionId,
            int resumeId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                // Verify that the job belongs to the logged-in user
                var job = await _context.JobDescriptions
                    .FirstOrDefaultAsync(j =>
                        j.JobDescriptionId == jobDescriptionId &&
                        j.UserId == userId);

                if (job == null)
                {
                    return NotFound();
                }

                // Verify that the selected CV belongs to the logged-in user
                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r =>
                        r.ResumeId == resumeId &&
                        r.UserId == userId);

                if (resume == null)
                {
                    return NotFound();
                }

                // Run the matching engine
                var jobMatch = await _jobMatchService.AnalyzeMatchAsync(
                    userId,
                    resumeId,
                    jobDescriptionId);

                // Save the result
                _context.JobMatches.Add(jobMatch);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Job matching completed successfully.";

                return RedirectToAction(
                    nameof(MatchResult),
                    new { id = jobMatch.JobMatchId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Job matching failed: " + ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id = jobDescriptionId });
            }
        }
        // GET: /Job/MatchResult/5
        [HttpGet]
        public async Task<IActionResult> MatchResult(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var match = await _context.JobMatches
                .Include(m => m.JobDescription)
                .Include(m => m.Resume)
                .FirstOrDefaultAsync(m =>
                    m.JobMatchId == id &&
                    m.UserId == userId);

            if (match == null)
            {
                return NotFound();
            }

            return View(match);
        }
        // GET: /Job/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var matches = await _context.JobMatches
                .Include(m => m.JobDescription)
                .Include(m => m.Resume)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(matches);
        }
        // POST: /Job/DeleteMatch/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var match = await _context.JobMatches
                .FirstOrDefaultAsync(m =>
                    m.JobMatchId == id &&
                    m.UserId == userId);

            if (match == null)
            {
                return NotFound();
            }

            _context.JobMatches.Remove(match);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Job match deleted successfully.";

            return RedirectToAction(nameof(History));
        }
    }
}