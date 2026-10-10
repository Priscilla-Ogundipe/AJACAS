using System.ComponentModel.DataAnnotations;

namespace AJACAS.Models
{
    public class JobMatch
    {
        [Key]
        public int JobMatchId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ResumeId { get; set; }

        [Required]
        public int JobDescriptionId { get; set; }

        [Range(0, 100)]
        public decimal? MatchPercentage { get; set; }

        public string? MatchedSkills { get; set; }

        public string? MissingSkills { get; set; }

        public string? MatchingExperience { get; set; }

        public string? SkillGapAnalysis { get; set; }

        public string? Analysis { get; set; }

        public string? AnalysisJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties

        public ApplicationUser? User { get; set; }

        public Resume? Resume { get; set; }

        public JobDescription? JobDescription { get; set; }
    }
}