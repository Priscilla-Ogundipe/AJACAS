using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AJACAS.ViewModels
{
    public class ResumeUploadViewModel
    {
        [Required]
        [Display(Name = "Resume Name")]
        public string ResumeName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "CV File")]
        public IFormFile? File { get; set; }
    }
}