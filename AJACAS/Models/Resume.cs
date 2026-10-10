using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AJACAS.Models
{
    public class Resume
    {
        [Key]
        public int ResumeId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string ResumeName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FileType { get; set; }

        public long? FileSize { get; set; }

        public string? ExtractedText { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<ResumeAnalysis> ResumeAnalyses { get; set; }
            = new List<ResumeAnalysis>();
    }
}