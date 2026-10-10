using System.ComponentModel.DataAnnotations;

namespace AJACAS.Models
{
    public class UserSkill
    {
        [Key]
        public int UserSkillId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int SkillId { get; set; }

        [StringLength(50)]
        public string? ProficiencyLevel { get; set; }

        public decimal? YearsOfExperience { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }

        public Skill? Skill { get; set; }
    }
}