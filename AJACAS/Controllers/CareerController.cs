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
    public class CareerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CareerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Career/Recommendations
        [HttpGet]
        public async Task<IActionResult> Recommendations()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // -----------------------------------------
            // GET USER SKILLS
            // -----------------------------------------

            var skillCount = await _context.UserSkills
                .CountAsync(s => s.UserId == userId);


            // -----------------------------------------
            // GET LATEST RESUME ANALYSIS
            // -----------------------------------------

            var latestAnalysis = await _context.ResumeAnalyses
                .Include(a => a.Resume)
                .Where(a =>
                    a.Resume != null &&
                    a.Resume.UserId == userId)
                .OrderByDescending(a => a.AnalyzedAt)
                .FirstOrDefaultAsync();


            // -----------------------------------------
            // GET JOB MATCHES
            // -----------------------------------------
            // -----------------------------------------
            // GET JOB MATCHES
            // -----------------------------------------

            var jobMatches = await _context.JobMatches
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var matchScores = jobMatches
                .Where(m => m.MatchPercentage.HasValue)
                .Select(m => m.MatchPercentage!.Value)
                .ToList();

            decimal? averageMatch = null;

            if (matchScores.Any())
            {
                averageMatch = matchScores.Average();
            }


            // -----------------------------------------
            // ANALYZE SKILL GAPS
            // -----------------------------------------

            var missingSkills = jobMatches
                .Select(m => m.MissingSkills)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var skillGaps = AnalyzeSkillGaps(missingSkills);

            // -----------------------------------------
            // CREATE MODEL
            // -----------------------------------------

            var model = new CareerRecommendationViewModel
            {
                SkillCount = skillCount,

                ResumeScore =
        latestAnalysis?.OverallScore,

                AverageJobMatch =
        averageMatch,

                HasResumeAnalysis =
        latestAnalysis != null,

                HasJobMatches =
        matchScores.Any(),

                SkillGaps =
        skillGaps
            };
            // -----------------------------------------
            // ADD DATA-DRIVEN SKILL RECOMMENDATIONS
            // -----------------------------------------

            foreach (var skillGap in model.SkillGaps)
            {
                model.SkillsToImprove.Add(
                    $"{skillGap.Skill} is missing from {skillGap.JobCount} of your analyzed job matches.");
            }
            if (model.SkillGaps.Any())
            {
                var topSkill = model.SkillGaps.First();

                model.SkillsToImprove.Insert(
                    0,
                    $"{topSkill.Skill} should be a priority because it is missing from {topSkill.JobCount} analyzed job match{(topSkill.JobCount == 1 ? "" : "es")}."
                );
            }


            // -----------------------------------------
            // RESUME RECOMMENDATIONS
            // -----------------------------------------

            if (latestAnalysis != null)
            {
                if (latestAnalysis.OverallScore >= 80)
                {
                    model.Strengths.Add(
                        "Your resume has a strong overall quality.");
                }
                else if (latestAnalysis.OverallScore >= 60)
                {
                    model.Strengths.Add(
                        "Your resume has a good foundation.");
                }
                else
                {
                    model.ResumeRecommendations.Add(
                        "Improve the overall quality and structure of your resume.");
                }


                if (latestAnalysis.AtsScore >= 80)
                {
                    model.Strengths.Add(
                        "Your resume has good ATS compatibility.");
                }
                else
                {
                    model.ResumeRecommendations.Add(
                        "Improve ATS compatibility by using clear headings, standard formatting and relevant keywords.");
                }


                if (latestAnalysis.KeywordScore >= 80)
                {
                    model.Strengths.Add(
                        "Your resume contains a good range of relevant keywords.");
                }
                else
                {
                    model.ResumeRecommendations.Add(
                        "Add more keywords relevant to the roles you are targeting.");
                }


                if (latestAnalysis.ContentScore >= 80)
                {
                    model.Strengths.Add(
                        "Your resume contains strong professional content.");
                }
                else
                {
                    model.ResumeRecommendations.Add(
                        "Improve the quality and relevance of your resume content.");
                }
            }
            else
            {
                model.ResumeRecommendations.Add(
                    "Upload and analyze a resume to receive personalized resume recommendations.");
            }


            // -----------------------------------------
            // JOB MATCH RECOMMENDATIONS
            // -----------------------------------------

            if (averageMatch.HasValue)
            {
                if (averageMatch >= 80)
                {
                    model.Strengths.Add(
                        "Your profile is highly compatible with the jobs you have analyzed.");
                }
                else if (averageMatch >= 60)
                {
                    model.ResumeRecommendations.Add(
                        "Your profile has moderate compatibility with your analyzed jobs. Consider improving your skill gaps.");
                }
                else
                {
                    model.ResumeRecommendations.Add(
                        "Your job compatibility is currently low. Consider developing additional technical skills.");
                }
            }


            // -----------------------------------------
            // SKILL RECOMMENDATIONS
            // -----------------------------------------

            if (skillCount < 5)
            {
                model.SkillsToImprove.Add("Add more relevant technical skills to your profile.");
            }

            if (skillCount < 10)
            {
                model.SkillsToImprove.Add("Expand your skill set with industry-relevant technologies.");
            }


            // General technology recommendations
            model.SkillsToImprove.Add("REST API development");
            model.SkillsToImprove.Add("Git and GitHub");
            model.SkillsToImprove.Add("Cloud fundamentals");


            // -----------------------------------------
            // RECOMMENDED CAREER ROLES
            // -----------------------------------------

            model.RecommendedRoles.Add(
                "Software Developer");

            model.RecommendedRoles.Add(
                "Web Application Developer");

            model.RecommendedRoles.Add(
                "Backend Developer");

            model.RecommendedRoles.Add(
                ".NET Developer");


            // -----------------------------------------
            // FALLBACK
            // -----------------------------------------

            if (!model.ResumeRecommendations.Any())
            {
                model.ResumeRecommendations.Add(
                    "Continue analyzing your resume and job matches to identify additional areas for improvement.");
            }

            if (!model.Strengths.Any())
            {
                model.Strengths.Add(
                    "Continue building your profile to unlock more personalized insights.");
            }


            return View(model);
        }
        private List<SkillGapViewModel> AnalyzeSkillGaps(
    List<string?> missingSkills)
        {
            var skillFrequency =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var missingSkillText in missingSkills)
            {
                if (string.IsNullOrWhiteSpace(missingSkillText))
                {
                    continue;
                }

                var skills = missingSkillText
                    .Split(
                        new[] { ',', ';', '\n', '\r', '|' },
                        StringSplitOptions.RemoveEmptyEntries);

                // Prevent the same skill from being counted
                // twice for one particular job.
                var uniqueSkills = skills
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var skill in uniqueSkills)
                {
                    if (skillFrequency.ContainsKey(skill))
                    {
                        skillFrequency[skill]++;
                    }
                    else
                    {
                        skillFrequency[skill] = 1;
                    }
                }
            }

            var result = skillFrequency
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key)
                .Take(10)
                .Select(x => new SkillGapViewModel
                {
                    Skill = x.Key,

                    JobCount = x.Value,

                    Priority = x.Value >= 3
                        ? "High"
                        : x.Value == 2
                            ? "Medium"
                            : "Low"
                })
                .ToList();

            return result;
        }
    }
}