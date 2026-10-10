using System.ComponentModel.DataAnnotations;

namespace AJACAS.ViewModels
{
    public class SkillViewModel
    {
        [Required]
        [Display(Name = "Skill")]
        public string SkillName { get; set; } = string.Empty;

        [Display(Name = "Category")]
        public string? Category { get; set; }

        [Required]
        [Display(Name = "Proficiency Level")]
        public string ProficiencyLevel { get; set; } = string.Empty;

        [Display(Name = "Years of Experience")]
        [Range(0, 100)]
        public decimal? YearsOfExperience { get; set; }
    }
}