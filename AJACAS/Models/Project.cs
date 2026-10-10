using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AJACAS.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Technologies { get; set; }

        [StringLength(500)]
        public string? ProjectUrl { get; set; }

        [StringLength(500)]
        public string? GitHubUrl { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}