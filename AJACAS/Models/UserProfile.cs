using System.ComponentModel.DataAnnotations;

namespace AJACAS.Models
{
    public class UserProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? ProfessionalTitle { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [StringLength(2000)]
        public string? ProfessionalSummary { get; set; }

        [StringLength(500)]
        public string? LinkedInUrl { get; set; }

        [StringLength(500)]
        public string? GitHubUrl { get; set; }

        [StringLength(500)]
        public string? PortfolioUrl { get; set; }

        [StringLength(500)]
        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ApplicationUser? User { get; set; }
    }
}