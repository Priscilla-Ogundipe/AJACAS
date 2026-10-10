using Microsoft.AspNetCore.Identity;

namespace AJACAS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public UserProfile? UserProfile { get; set; }

        public ICollection<UserSkill> UserSkills { get; set; }
            = new List<UserSkill>();

        public ICollection<Education> Educations { get; set; }
            = new List<Education>();

        public ICollection<WorkExperience> WorkExperiences { get; set; }
            = new List<WorkExperience>();

        public ICollection<Project> Projects { get; set; }
            = new List<Project>();
        public ICollection<Resume> Resumes { get; set; }
           = new List<Resume>();
        public ICollection<JobDescription> JobDescriptions { get; set; }
    = new List<JobDescription>();
    }
}