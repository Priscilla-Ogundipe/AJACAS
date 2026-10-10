namespace AJACAS.Models
{
    public class AIResumeAnalysisResult
    {
        public int OverallScore { get; set; }

        public int AtsScore { get; set; }

        public int ContentScore { get; set; }

        public int FormattingScore { get; set; }

        public int KeywordScore { get; set; }

        public List<string> Strengths { get; set; } = new();

        public List<string> Weaknesses { get; set; } = new();

        public List<string> Recommendations { get; set; } = new();

        public List<string> MissingKeywords { get; set; } = new();
    }
}