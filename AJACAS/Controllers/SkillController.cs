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
    public class SkillController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SkillController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Skill
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userSkills = await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserId == userId)
                .OrderBy(us => us.Skill!.SkillName)
                .ToListAsync();

            return View(userSkills);
        }

        // GET: /Skill/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Skill/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SkillViewModel model)
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

            // Check if skill already exists
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s =>
                    s.SkillName.ToLower() == model.SkillName.ToLower());

            // If skill doesn't exist, create it
            if (skill == null)
            {
                skill = new Skill
                {
                    SkillName = model.SkillName.Trim(),
                    Category = model.Category,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Skills.Add(skill);

                await _context.SaveChangesAsync();
            }

            // Check whether this user already has this skill
            var existingUserSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us =>
                    us.UserId == userId &&
                    us.SkillId == skill.SkillId);

            if (existingUserSkill != null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You have already added this skill.");

                return View(model);
            }

            var userSkill = new UserSkill
            {
                UserId = userId,
                SkillId = skill.SkillId,
                ProficiencyLevel = model.ProficiencyLevel,
                YearsOfExperience = model.YearsOfExperience
            };

            _context.UserSkills.Add(userSkill);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Skill/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userSkill = await _context.UserSkills
                .Include(us => us.Skill)
                .FirstOrDefaultAsync(us =>
                    us.UserSkillId == id &&
                    us.UserId == userId);

            if (userSkill == null)
            {
                return NotFound();
            }

            var model = new SkillViewModel
            {
                SkillName = userSkill.Skill!.SkillName,
                Category = userSkill.Skill.Category,
                ProficiencyLevel =
                    userSkill.ProficiencyLevel ?? string.Empty,
                YearsOfExperience =
                    userSkill.YearsOfExperience
            };

            return View(model);
        }

        // POST: /Skill/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SkillViewModel model)
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

            var userSkill = await _context.UserSkills
                .Include(us => us.Skill)
                .FirstOrDefaultAsync(us =>
                    us.UserSkillId == id &&
                    us.UserId == userId);

            if (userSkill == null)
            {
                return NotFound();
            }

            userSkill.ProficiencyLevel =
                model.ProficiencyLevel;

            userSkill.YearsOfExperience =
                model.YearsOfExperience;

            userSkill.Skill!.Category =
                model.Category;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Skill/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us =>
                    us.UserSkillId == id &&
                    us.UserId == userId);

            if (userSkill == null)
            {
                return NotFound();
            }

            _context.UserSkills.Remove(userSkill);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Skill removed successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}