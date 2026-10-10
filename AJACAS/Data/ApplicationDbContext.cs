using AJACAS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AJACAS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }

        public DbSet<Skill> Skills { get; set; }

        public DbSet<UserSkill> UserSkills { get; set; }

        public DbSet<Education> Educations { get; set; }

        public DbSet<WorkExperience> WorkExperiences { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Resume> Resumes { get; set; }

        public DbSet<ResumeAnalysis> ResumeAnalyses { get; set; }

        public DbSet<JobDescription> JobDescriptions { get; set; }

        public DbSet<JobMatch> JobMatches { get; set; }

        //public DbSet<CoverLetter> CoverLetters { get; set; }

        //public DbSet<JobApplication> JobApplications { get; set; }

        //public DbSet<AIRecommendation> AIRecommendations { get; set; }

        //public DbSet<InterviewSession> InterviewSessions { get; set; }

        //public DbSet<InterviewQuestion> InterviewQuestions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // UserSkill → ApplicationUser
            builder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSkill → Skill
            builder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
            // UserProfile → ApplicationUser
            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
               .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSkill → ApplicationUser
            builder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserSkill → Skill
            builder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Restrict);

            // Education → ApplicationUser
            builder.Entity<Education>()
                .HasOne(e => e.User)
                .WithMany(u => u.Educations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // WorkExperience → ApplicationUser
            builder.Entity<WorkExperience>()
                .HasOne(w => w.User)
                .WithMany(u => u.WorkExperiences)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Project → ApplicationUser
            builder.Entity<Project>()
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Resume → ApplicationUser
            builder.Entity<Resume>()
                .HasOne(r => r.User)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ResumeAnalysis → Resume
            builder.Entity<ResumeAnalysis>()
                .HasOne(ra => ra.Resume)
                .WithMany(r => r.ResumeAnalyses)
                .HasForeignKey(ra => ra.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobDescription → ApplicationUser
            builder.Entity<JobDescription>()
                .HasOne(j => j.User)
                .WithMany(u => u.JobDescriptions)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobMatch → ApplicationUser
            builder.Entity<JobMatch>()
                .HasOne(jm => jm.User)
                .WithMany()
                .HasForeignKey(jm => jm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // JobMatch → JobDescription
            builder.Entity<JobMatch>()
                .HasOne(jm => jm.JobDescription)
                .WithMany(j => j.JobMatches)
                .HasForeignKey(jm => jm.JobDescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // JobMatch → Resume
            builder.Entity<JobMatch>()
                .HasOne(jm => jm.Resume)
                .WithMany()
                .HasForeignKey(jm => jm.ResumeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique skill name
            builder.Entity<Skill>()
                .HasIndex(s => s.SkillName)
                .IsUnique();

            // Unique user/skill combination
            builder.Entity<UserSkill>()
                .HasIndex(us => new { us.UserId, us.SkillId })
                .IsUnique();
        }
    }
}