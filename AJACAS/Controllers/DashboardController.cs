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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // ==========================================
            // USER PROFILE
            // ==========================================

            var user = await _userManager.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            // ==========================================
            // RESUMES
            // ==========================================

            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();

            // ==========================================
            // RESUME ANALYSES
            // ==========================================

            var analyses = await _context.ResumeAnalyses
                .Include(a => a.Resume)
                .Where(a =>
                    a.Resume != null &&
                    a.Resume.UserId == userId)
                .OrderByDescending(a => a.AnalyzedAt)
                .ToListAsync();

            // ==========================================
            // JOB MATCHES
            // ==========================================

            var jobMatches = await _context.JobMatches
                .Include(m => m.JobDescription)
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // ==========================================
            // RESUME SCORE HISTORY
            // ==========================================

            var resumeScores = analyses
                .Where(a =>
                    a.Resume != null &&
                    a.OverallScore.HasValue)
                .OrderBy(a => a.AnalyzedAt)
                .Select(a => new ResumeScoreChartViewModel
                {
                    ResumeName = a.Resume!.ResumeName,
                    Score = a.OverallScore!.Value,
                    Date = a.AnalyzedAt
                })
                .ToList();


            // ==========================================
            // JOB MATCH HISTORY
            // ==========================================

            var matchHistory = jobMatches
                .Where(m => m.MatchPercentage.HasValue)
                .OrderBy(m => m.CreatedAt)
                .TakeLast(10)
                .Select(m => new JobMatchChartViewModel
                {
                    JobTitle =
                        m.JobDescription?.JobTitle ?? "Job",

                    MatchPercentage =
                        m.MatchPercentage!.Value,

                    Date =
                        m.CreatedAt
                })
                .ToList();
            // ==========================================
            // USER SKILLS
            // ==========================================

            var skillCount = await _context.UserSkills
                .CountAsync(s => s.UserId == userId);

            // ==========================================
            // AVERAGE MATCH
            // ==========================================

            decimal? averageMatch = null;

            var matchScores = jobMatches
                .Where(m => m.MatchPercentage.HasValue)
                .Select(m => m.MatchPercentage!.Value)
                .ToList();

            if (matchScores.Any())
            {
                averageMatch =
                    Math.Round(
                        matchScores.Average(),
                        2);
            }

            // ==========================================
            // LATEST RESUME SCORE
            // ==========================================

            var latestAnalysis = analyses.FirstOrDefault();

            // ==========================================
            // RECENT MATCHES
            // ==========================================

            var recentMatches = jobMatches
                .Take(5)
                .Select(m => new RecentJobMatchViewModel
                {
                    JobMatchId =
                        m.JobMatchId,

                    JobTitle =
                        m.JobDescription?.JobTitle,

                    CompanyName =
                        m.JobDescription?.CompanyName,

                    MatchPercentage =
                        m.MatchPercentage,

                    CreatedAt =
                        m.CreatedAt
                })
                .ToList();

            // ==========================================
            // SKILL GAP ANALYSIS
            // ==========================================

            var skillFrequency =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (var match in jobMatches)
            {
                if (string.IsNullOrWhiteSpace(
                    match.MissingSkills))
                {
                    continue;
                }

                var skills = match.MissingSkills
                    .Split(
                        new[]
                        {
                            ',',
                            ';',
                            '\n',
                            '\r',
                            '|'
                        },
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s =>
                        !string.IsNullOrWhiteSpace(s))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase);

                foreach (var skill in skills)
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

            var skillGaps = skillFrequency
                .OrderByDescending(x => x.Value)
                .Take(5)
                .Select(x =>
                    new DashboardSkillGapViewModel
                    {
                        Skill = x.Key,
                        Count = x.Value
                    })
                .ToList();

            // ==========================================
            // CREATE VIEW MODEL
            // ==========================================

            var model = new DashboardViewModel
            {
                FirstName =
                    user?.UserProfile?.FirstName,

                ResumeCount =
                    resumes.Count,

                AnalysisCount =
                    analyses.Count,

                JobMatchCount =
                    jobMatches.Count,

                AverageMatchPercentage =
                    averageMatch,

                LatestResumeScore =
                    latestAnalysis?.OverallScore,

                LatestResumeName =
                    latestAnalysis?.Resume?.ResumeName,

                SkillCount =
                    skillCount,

                RecentMatches =
                    recentMatches,

                SkillGaps =
                    skillGaps,
                ResumeScores = resumeScores,

                MatchHistory = matchHistory,
            };

            return View(model);
        }
    }
}