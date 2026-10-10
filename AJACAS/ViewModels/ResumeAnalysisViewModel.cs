using System.ComponentModel.DataAnnotations;

namespace AJACAS.ViewModels
{
    public class ResumeAnalysisViewModel
    {
        public int AnalysisId { get; set; }

        public int ResumeId { get; set; }

        public string ResumeName { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal? OverallScore { get; set; }

        [Range(0, 100)]
        public decimal? AtsScore { get; set; }

        [Range(0, 100)]
        public decimal? ContentScore { get; set; }

        [Range(0, 100)]
        public decimal? FormattingScore { get; set; }

        [Range(0, 100)]
        public decimal? KeywordScore { get; set; }

        public string? Strengths { get; set; }

        public string? Weaknesses { get; set; }

        public string? Recommendations { get; set; }

        public string? MissingKeywords { get; set; }

        public DateTime? AnalyzedAt { get; set; }
    }
}