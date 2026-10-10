using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AJACAS.Models
{
    public class WorkExperience
    {
        [Key]
        public int WorkExperienceId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Location { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        [StringLength(4000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}