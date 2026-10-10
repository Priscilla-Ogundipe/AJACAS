namespace AJACAS.ViewModels
{
    public class DashboardViewModel
    {
        public string? FirstName { get; set; }

        public int ResumeCount { get; set; }

        public int AnalysisCount { get; set; }

        public int JobMatchCount { get; set; }

        public decimal? AverageMatchPercentage { get; set; }

        public decimal? LatestResumeScore { get; set; }

        public string? LatestResumeName { get; set; }

        public int SkillCount { get; set; }

        public List<RecentJobMatchViewModel> RecentMatches { get; set; }
            = new();

        public List<DashboardSkillGapViewModel> SkillGaps { get; set; }
            = new();

        // New: Resume analysis chart
        public List<ResumeScoreChartViewModel> ResumeScores { get; set; }
            = new();

        // New: Job match chart
        public List<JobMatchChartViewModel> MatchHistory { get; set; }
            = new();
    }

    public class RecentJobMatchViewModel
    {
        public int JobMatchId { get; set; }

        public string? JobTitle { get; set; }

        public string? CompanyName { get; set; }

        public decimal? MatchPercentage { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class DashboardSkillGapViewModel
    {
        public string Skill { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class ResumeScoreChartViewModel
    {
        public string ResumeName { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public DateTime Date { get; set; }
    }

    public class JobMatchChartViewModel
    {
        public string JobTitle { get; set; } = string.Empty;

        public decimal MatchPercentage { get; set; }

        public DateTime Date { get; set; }
    }
}