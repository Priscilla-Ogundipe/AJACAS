namespace AJACAS.ViewModels
{
    public class CareerRecommendationViewModel
    {
        public List<string> RecommendedRoles { get; set; }
            = new();

        public List<string> SkillsToImprove { get; set; }
            = new();

        public List<string> ResumeRecommendations { get; set; }
            = new();

        public List<string> Strengths { get; set; }
            = new();

        public List<SkillGapViewModel> SkillGaps { get; set; }
            = new();

        public decimal? ResumeScore { get; set; }

        public decimal? AverageJobMatch { get; set; }

        public int SkillCount { get; set; }

        public bool HasResumeAnalysis { get; set; }

        public bool HasJobMatches { get; set; }
    }


    public class SkillGapViewModel
    {
        public string Skill { get; set; } = string.Empty;

        public int JobCount { get; set; }

        public string Priority { get; set; } = string.Empty;
    }
}