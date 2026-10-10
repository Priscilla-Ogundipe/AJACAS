using AJACAS.Data;
using AJACAS.Models;
using AJACAS.Services;
using AJACAS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace AJACAS.Controllers
{
    [Authorize]
    public class ResumeAnalysisController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AIResumeAnalysisService _aiService;

        public ResumeAnalysisController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AIResumeAnalysisService aiService)
        {
            _context = context;
            _userManager = userManager;
            _aiService = aiService;
        }

        // GET: /ResumeAnalysis
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var analyses = await _context.ResumeAnalyses
                .Include(a => a.Resume)
                .Where(a => a.Resume!.UserId == userId)
                .OrderByDescending(a => a.AnalyzedAt)
                .Select(a => new ResumeAnalysisViewModel
                {
                    AnalysisId = a.AnalysisId,
                    ResumeId = a.ResumeId,

                    ResumeName = a.Resume!.ResumeName,

                    OverallScore = a.OverallScore,
                    AtsScore = a.AtsScore,
                    ContentScore = a.ContentScore,
                    FormattingScore = a.FormattingScore,
                    KeywordScore = a.KeywordScore,

                    Strengths = a.Strengths,
                    Weaknesses = a.Weaknesses,
                    Recommendations = a.Recommendations,
                    MissingKeywords = a.MissingKeywords,

                    AnalyzedAt = a.AnalyzedAt
                })
                .ToListAsync();

            return View(analyses);
        }

        // GET: /ResumeAnalysis/Details/5
       [HttpGet]
public async Task<IActionResult> Details(int id)
{
    var userId = _userManager.GetUserId(User);

    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized();
    }

    var analysis = await _context.ResumeAnalyses
        .Include(a => a.Resume)
        .FirstOrDefaultAsync(a =>
            a.AnalysisId == id &&
            a.Resume!.UserId == userId);

    if (analysis == null)
    {
        return NotFound();
    }

    var model = new ResumeAnalysisViewModel
    {
        AnalysisId = analysis.AnalysisId,
        ResumeId = analysis.ResumeId,

        ResumeName = analysis.Resume!.ResumeName,

        OverallScore = analysis.OverallScore,
        AtsScore = analysis.AtsScore,
        ContentScore = analysis.ContentScore,
        FormattingScore = analysis.FormattingScore,
        KeywordScore = analysis.KeywordScore,

        Strengths = analysis.Strengths,
        Weaknesses = analysis.Weaknesses,
        Recommendations = analysis.Recommendations,
        MissingKeywords = analysis.MissingKeywords,

        AnalyzedAt = analysis.AnalyzedAt
    };

    return View(model);
}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(int resumeId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r =>
                    r.ResumeId == resumeId &&
                    r.UserId == userId);

            if (resume == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(resume.ExtractedText))
            {
                TempData["ErrorMessage"] =
                    "This CV does not contain extracted text.";

                return RedirectToAction(
                    "Index",
                    "Resume");
            }

            try
            {
                var aiResult = await _aiService
     .AnalyzeResumeAsync(resume.ExtractedText);

                var result = JsonSerializer.Deserialize<AIResumeAnalysisResult>(
                    aiResult,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null)
                {
                    throw new Exception(
                        "The AI returned an invalid analysis result.");
                }

                var analysis = new ResumeAnalysis
                {
                    ResumeId = resume.ResumeId,

                    OverallScore = result.OverallScore,
                    AtsScore = result.AtsScore,
                    ContentScore = result.ContentScore,
                    FormattingScore = result.FormattingScore,
                    KeywordScore = result.KeywordScore,

                    Strengths = string.Join(
                        "\n",
                        result.Strengths),

                    Weaknesses = string.Join(
                        "\n",
                        result.Weaknesses),

                    Recommendations = string.Join(
                        "\n",
                        result.Recommendations),

                    MissingKeywords = string.Join(
                        ", ",
                        result.MissingKeywords),

                    AnalysisJson = aiResult,

                    AnalyzedAt = DateTime.UtcNow
                };

                _context.ResumeAnalyses.Add(analysis);

                await _context.SaveChangesAsync();

                return RedirectToAction(
                    nameof(Details),
                    new { id = analysis.AnalysisId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    "Analysis failed: " + ex.Message;

                return RedirectToAction(
                    "Index",
                    "Resume");
            }
        }
    }
}