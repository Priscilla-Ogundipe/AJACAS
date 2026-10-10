using System.ComponentModel.DataAnnotations;

namespace AJACAS.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Professional Title")]
        [StringLength(150)]
        public string? ProfessionalTitle { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        [Display(Name = "Phone Number")]
        [StringLength(30)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Professional Summary")]
        [StringLength(2000)]
        public string? ProfessionalSummary { get; set; }

        [Display(Name = "LinkedIn URL")]
        [StringLength(500)]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "GitHub URL")]
        [StringLength(500)]
        public string? GitHubUrl { get; set; }

        [Display(Name = "Portfolio URL")]
        [StringLength(500)]
        public string? PortfolioUrl { get; set; }
    }
}