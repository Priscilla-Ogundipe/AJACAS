using System.ComponentModel.DataAnnotations;

namespace AJACAS.Models
{
    public class JobDescription
    {
        [Key]
        public int JobDescriptionId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? JobUrl { get; set; }

        [StringLength(100)]
        public string? EmploymentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }

        public ICollection<JobMatch> JobMatches { get; set; }
            = new List<JobMatch>();
    }
}