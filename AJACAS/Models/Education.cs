using AJACAS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AJACAS.Models
{
    public class Education
    {
        [Key]
        public int EducationId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Institution { get; set; } = string.Empty;

        public string? Degree { get; set; }

        public string? FieldOfStudy { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}