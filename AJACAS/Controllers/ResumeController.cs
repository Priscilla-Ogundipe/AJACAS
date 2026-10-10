using AJACAS.Data;
using AJACAS.Models;
using AJACAS.Services;
using AJACAS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AJACAS.Controllers
{
    [Authorize]
    public class ResumeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ResumeTextExtractor _textExtractor;

        public ResumeController(
     ApplicationDbContext context,
     UserManager<ApplicationUser> userManager,
     IWebHostEnvironment environment,
     ResumeTextExtractor textExtractor)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _textExtractor = textExtractor;
        }

        // GET: /Resume
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            return View(resumes);
        }

        // GET: /Resume/Upload
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // POST: /Resume/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ResumeUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a CV file.");
                return View(model);
            }

            var allowedExtensions = new[]
            {
                ".pdf",
                ".doc",
                ".docx"
            };

            var extension = Path.GetExtension(model.File.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    "File",
                    "Only PDF, DOC, and DOCX files are allowed.");

                return View(model);
            }

            // 10 MB maximum
            if (model.File.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "File",
                    "The maximum file size is 10 MB.");

                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Create uploads/resumes folder
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "resumes");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Generate unique file name
            var uniqueFileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                uniqueFileName);

            // Save file
            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }
            string extractedText = _textExtractor.ExtractText(
    filePath,
    extension);

            // Make previous resumes inactive
            var existingResumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .ToListAsync();

            foreach (var resume in existingResumes)
            {
                resume.IsActive = false;
            }

            // Save database record
            var newResume = new Resume
            {
                UserId = userId,

                ResumeName = model.ResumeName,

                FileName = model.File.FileName,

                FilePath = $"/uploads/resumes/{uniqueFileName}",

                ExtractedText = extractedText,

                FileType = extension,

                FileSize = model.File.Length,

                IsActive = true,

                UploadedAt = DateTime.UtcNow
            };

            _context.Resumes.Add(newResume);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your CV has been uploaded successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Resume/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Find the CV belonging to the currently logged-in user
            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r =>
                    r.ResumeId == id &&
                    r.UserId == userId);

            if (resume == null)
            {
                TempData["ErrorMessage"] =
                    "CV not found, or you do not have permission to delete it.";

                return RedirectToAction(nameof(Index));
            }

            // Get the physical file path
            var relativePath = resume.FilePath?
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                var physicalPath = Path.Combine(
                    _environment.WebRootPath,
                    relativePath);

                // Only delete files inside wwwroot/uploads/resumes
                var uploadFolder = Path.GetFullPath(
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "resumes"));

                var fullPath = Path.GetFullPath(physicalPath);

                var folderPrefix = uploadFolder
                    .TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (fullPath.StartsWith(
                        folderPrefix,
                        StringComparison.OrdinalIgnoreCase)
                    && System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            // Delete the database record
            _context.Resumes.Remove(resume);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your CV has been deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

    }
}