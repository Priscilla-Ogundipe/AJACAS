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
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return RedirectToAction(nameof(Edit));
            }

            var model = new ProfileViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                ProfessionalTitle = profile.ProfessionalTitle,
                Location = profile.Location,
                PhoneNumber = profile.PhoneNumber,
                ProfessionalSummary = profile.ProfessionalSummary,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,
                PortfolioUrl = profile.PortfolioUrl
            };

            return View(model);
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return View(new ProfileViewModel());
            }

            var model = new ProfileViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                ProfessionalTitle = profile.ProfessionalTitle,
                Location = profile.Location,
                PhoneNumber = profile.PhoneNumber,
                ProfessionalSummary = profile.ProfessionalSummary,
                LinkedInUrl = profile.LinkedInUrl,
                GitHubUrl = profile.GitHubUrl,
                PortfolioUrl = profile.PortfolioUrl
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
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

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    ProfessionalTitle = model.ProfessionalTitle,
                    Location = model.Location,
                    PhoneNumber = model.PhoneNumber,
                    ProfessionalSummary = model.ProfessionalSummary,
                    LinkedInUrl = model.LinkedInUrl,
                    GitHubUrl = model.GitHubUrl,
                    PortfolioUrl = model.PortfolioUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserProfiles.Add(profile);
            }
            else
            {
                profile.FirstName = model.FirstName;
                profile.LastName = model.LastName;
                profile.ProfessionalTitle = model.ProfessionalTitle;
                profile.Location = model.Location;
                profile.PhoneNumber = model.PhoneNumber;
                profile.ProfessionalSummary = model.ProfessionalSummary;
                profile.LinkedInUrl = model.LinkedInUrl;
                profile.GitHubUrl = model.GitHubUrl;
                profile.PortfolioUrl = model.PortfolioUrl;
                profile.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your profile has been saved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}