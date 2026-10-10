using System.ComponentModel.DataAnnotations;

namespace AJACAS.ViewModels
{
    public class JobDescriptionViewModel
    {
        [Required]
        [StringLength(250)]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [StringLength(150)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Required]
        [Display(Name = "Job Description")]
        public string Description { get; set; } = string.Empty;
    }
}