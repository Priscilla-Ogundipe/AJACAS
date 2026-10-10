using System.ComponentModel.DataAnnotations;

namespace AJACAS.Models
{
    public class Skill
    {
        [Key]
        public int SkillId { get; set; }

        [Required]
        [StringLength(150)]
        public string SkillName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Category { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ICollection<UserSkill> UserSkills { get; set; }
            = new List<UserSkill>();
    }
}