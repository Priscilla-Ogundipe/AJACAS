using AJACAS.Data;
using AJACAS.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AJACAS.Services
{
    public class JobMatchService
    {
        private readonly ApplicationDbContext _context;

        public JobMatchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobMatch> AnalyzeMatchAsync(
            string userId,
            int resumeId,
            int jobDescriptionId)
        {
            // ==================================================
            // 1. GET RESUME
            // ==================================================

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r =>
                    r.ResumeId == resumeId &&
                    r.UserId == userId);

            if (resume == null)
            {
                throw new Exception("Resume not found.");
            }

            if (string.IsNullOrWhiteSpace(resume.ExtractedText))
            {
                throw new Exception(
                    "The selected CV does not contain extracted text.");
            }


            // ==================================================
            // 2. GET JOB
            // ==================================================

            var job = await _context.JobDescriptions
                .FirstOrDefaultAsync(j =>
                    j.JobDescriptionId == jobDescriptionId &&
                    j.UserId == userId);

            if (job == null)
            {
                throw new Exception("Job description not found.");
            }


            // ==================================================
            // 3. NORMALIZE TEXT
            // ==================================================

            string jobText = string.Join(
                " ",
                job.JobTitle,
                job.CompanyName ?? "",
                job.Location ?? "",
                job.EmploymentType ?? "",
                job.Description
            );

            string normalizedJobText =
                NormalizeText(jobText);

            string normalizedResumeText =
                NormalizeText(resume.ExtractedText);


            // ==================================================
            // 4. GET USER SKILLS
            // ==================================================

            var userSkills = await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            var skillNames = userSkills
                .Where(us => us.Skill != null)
                .Select(us => us.Skill!.SkillName.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();


            // ==================================================
            // 5. IDENTIFY JOB SKILLS
            // ==================================================

            var commonSkills = GetCommonSkills();

            var jobSkills = commonSkills
                .Where(skill =>
                    ContainsKeyword(
                        normalizedJobText,
                        skill))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();


            // ==================================================
            // 6. MATCH SKILLS
            // ==================================================

            var matchedSkills = new List<string>();
            var missingSkills = new List<string>();

            foreach (var jobSkill in jobSkills)
            {
                bool userHasSkill =
                    skillNames.Any(userSkill =>
                        SkillsMatch(userSkill, jobSkill));

                bool appearsInResume =
                    ContainsKeyword(
                        normalizedResumeText,
                        jobSkill);

                if (userHasSkill || appearsInResume)
                {
                    if (!matchedSkills.Any(s =>
                        s.Equals(
                            jobSkill,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        matchedSkills.Add(jobSkill);
                    }
                }
                else
                {
                    missingSkills.Add(jobSkill);
                }
            }


            // ==================================================
            // 7. SKILL SCORE
            // ==================================================

            decimal skillScore;

            if (jobSkills.Count == 0)
            {
                skillScore = 50;
            }
            else
            {
                skillScore =
                    ((decimal)matchedSkills.Count /
                    jobSkills.Count) * 100;

                skillScore =
                    Math.Clamp(
                        skillScore,
                        0,
                        100);
            }


            // ==================================================
            // 8. EXPERIENCE SCORE
            // ==================================================

            var workExperiences =
                await _context.WorkExperiences
                    .Where(w => w.UserId == userId)
                    .ToListAsync();

            decimal experienceScore =
                CalculateExperienceScore(
                    normalizedJobText,
                    normalizedResumeText,
                    workExperiences);


            // ==================================================
            // 9. EDUCATION SCORE
            // ==================================================

            var educations =
                await _context.Educations
                    .Where(e => e.UserId == userId)
                    .ToListAsync();

            decimal educationScore =
                CalculateEducationScore(
                    normalizedJobText,
                    educations);


            // ==================================================
            // 10. PROJECT SCORE
            // ==================================================

            var projects =
                await _context.Projects
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

            decimal projectScore =
                CalculateProjectScore(
                    normalizedJobText,
                    projects);


            // ==================================================
            // 11. FINAL MATCH SCORE
            // ==================================================

            decimal matchPercentage =
                (skillScore * 0.50m) +
                (experienceScore * 0.25m) +
                (educationScore * 0.10m) +
                (projectScore * 0.15m);

            matchPercentage =
                Math.Round(
                    Math.Clamp(
                        matchPercentage,
                        0,
                        100),
                    2);


            // ==================================================
            // 12. EXPERIENCE ANALYSIS
            // ==================================================

            string matchingExperience;

            if (experienceScore >= 80)
            {
                matchingExperience =
                    "The candidate demonstrates strong relevant "
                    + "experience for this position.";
            }
            else if (experienceScore >= 60)
            {
                matchingExperience =
                    "The candidate demonstrates some relevant "
                    + "experience, but additional relevant experience "
                    + "may strengthen the application.";
            }
            else
            {
                matchingExperience =
                    "The CV provides limited evidence of directly "
                    + "relevant experience for this position.";
            }


            // ==================================================
            // 13. SKILL GAP ANALYSIS
            // ==================================================

            string skillGapAnalysis;

            if (missingSkills.Count == 0)
            {
                skillGapAnalysis =
                    "No major technical skill gaps were identified "
                    + "from the available job requirements.";
            }
            else
            {
                skillGapAnalysis =
                    "The candidate may need to develop or demonstrate "
                    + "the following skills: "
                    + string.Join(", ", missingSkills)
                    + ".";
            }


            // ==================================================
            // 14. GENERAL ANALYSIS
            // ==================================================

            string analysis;

            if (matchPercentage >= 80)
            {
                analysis =
                    "This CV is a strong match for the selected job. "
                    + "The candidate demonstrates a high level of "
                    + "alignment with the identified skills, experience, "
                    + "education and project requirements.";
            }
            else if (matchPercentage >= 60)
            {
                analysis =
                    "This CV is a moderate match for the selected job. "
                    + "The candidate meets several requirements, but "
                    + "some areas should be strengthened before applying.";
            }
            else
            {
                analysis =
                    "This CV currently has a relatively low match with "
                    + "the selected job. The candidate should consider "
                    + "developing relevant skills and tailoring the CV "
                    + "to the job requirements.";
            }


            // ==================================================
            // 15. CREATE ANALYSIS JSON
            // ==================================================

            var jsonData = new
            {
                MatchPercentage = matchPercentage,

                SkillScore = skillScore,

                ExperienceScore = experienceScore,

                EducationScore = educationScore,

                ProjectScore = projectScore,

                MatchedSkills = matchedSkills,

                MissingSkills = missingSkills
            };

            string analysisJson =
                JsonSerializer.Serialize(
                    jsonData,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });


            // ==================================================
            // 16. CREATE JOB MATCH
            // ==================================================

            var jobMatch = new JobMatch
            {
                UserId = userId,

                ResumeId = resumeId,

                JobDescriptionId =
                    jobDescriptionId,

                MatchPercentage =
                    matchPercentage,

                MatchedSkills =
                    string.Join(
                        ", ",
                        matchedSkills),

                MissingSkills =
                    string.Join(
                        ", ",
                        missingSkills),

                MatchingExperience =
                    matchingExperience,

                SkillGapAnalysis =
                    skillGapAnalysis,

                Analysis =
                    analysis,

                AnalysisJson =
                    analysisJson,

                CreatedAt =
                    DateTime.UtcNow
            };

            return jobMatch;
        }


        // ======================================================
        // COMMON SKILLS
        // ======================================================

        private static string[] GetCommonSkills()
        {
            return new[]
            {
                "C#",
                "ASP.NET",
                "ASP.NET Core",
                "ASP.NET MVC",
                "MVC",
                "Entity Framework",
                "Entity Framework Core",
                "SQL",
                "SQL Server",
                "MySQL",
                "PostgreSQL",

                "Java",
                "Python",
                "JavaScript",
                "TypeScript",

                "React",
                "Angular",
                "Vue",
                "Node.js",
                "Node",

                "HTML",
                "CSS",
                "Bootstrap",

                "Git",
                "GitHub",

                "Docker",

                "Azure",
                "AWS",

                "REST API",
                "Web API",
                "API",

                "Microsoft Office",
                "Microsoft Excel",
                "Excel",
                "Power BI",

                "Machine Learning",
                "Artificial Intelligence",
                "TensorFlow",
                "Keras",

                "PHP",
                "Laravel",
                "Django",

                "Flutter",
                "Kotlin",

                "Figma",
                "UI/UX",

                "Agile",
                "Scrum",

                "Jira",

                "CI/CD",

                "Linux",

                "Cloud Computing",

                "Unit Testing",

                "GitLab",

                "Postman"
            };
        }


        // ======================================================
        // NORMALIZE TEXT
        // ======================================================

        private static string NormalizeText(
            string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = text.ToLowerInvariant();

            text = text
                .Replace(".net", " dotnet ")
                .Replace("c#", " csharp ")
                .Replace("c++", " cpp ");

            text = Regex.Replace(
                text,
                @"[^\w\s]",
                " ");

            text = Regex.Replace(
                text,
                @"\s+",
                " ");

            return text.Trim();
        }


        // ======================================================
        // KEYWORD MATCHING
        // ======================================================

        private static bool ContainsKeyword(
            string text,
            string keyword)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            string normalizedKeyword =
                NormalizeText(keyword);

            if (string.IsNullOrWhiteSpace(
                normalizedKeyword))
            {
                return false;
            }

            string pattern =
                $@"(?<!\w){Regex.Escape(normalizedKeyword)}(?!\w)";

            return Regex.IsMatch(
                text,
                pattern,
                RegexOptions.IgnoreCase);
        }


        // ======================================================
        // SKILL MATCHING
        // ======================================================

        private static bool SkillsMatch(
            string userSkill,
            string jobSkill)
        {
            string user =
                NormalizeText(userSkill);

            string job =
                NormalizeText(jobSkill);

            if (user == job)
            {
                return true;
            }

            var equivalentSkills =
                new Dictionary<string, string[]>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "asp net core",
                        new[]
                        {
                            "asp net",
                            "asp net mvc",
                            "asp net core",
                            "dotnet"
                        }
                    },

                    {
                        "entity framework core",
                        new[]
                        {
                            "entity framework",
                            "entity framework core"
                        }
                    },

                    {
                        "web api",
                        new[]
                        {
                            "web api",
                            "rest api",
                            "api"
                        }
                    },

                    {
                        "sql server",
                        new[]
                        {
                            "sql server",
                            "sql"
                        }
                    },

                    {
                        "node js",
                        new[]
                        {
                            "node",
                            "node js"
                        }
                    }
                };

            if (equivalentSkills.TryGetValue(
                job,
                out var equivalents))
            {
                return equivalents.Any(
                    e => user == NormalizeText(e));
            }

            return false;
        }


        // ======================================================
        // EXPERIENCE SCORE
        // ======================================================

        private static decimal CalculateExperienceScore(
            string jobText,
            string resumeText,
            List<WorkExperience> workExperiences)
        {
            if (string.IsNullOrWhiteSpace(
                resumeText))
            {
                return 0;
            }

            int relevantSkillMatches = 0;

            var experienceTerms = new[]
            {
                "developer",
                "software",
                "programming",
                "development",
                "engineering",
                "backend",
                "frontend",
                "full stack",
                "web",
                "database",
                "api",
                "application",
                "system",
                "technical"
            };

            foreach (var term in experienceTerms)
            {
                if (ContainsKeyword(jobText, term) &&
                    ContainsKeyword(resumeText, term))
                {
                    relevantSkillMatches++;
                }
            }

            decimal baseScore;

            if (relevantSkillMatches >= 7)
            {
                baseScore = 90;
            }
            else if (relevantSkillMatches >= 5)
            {
                baseScore = 80;
            }
            else if (relevantSkillMatches >= 3)
            {
                baseScore = 70;
            }
            else if (relevantSkillMatches >= 1)
            {
                baseScore = 55;
            }
            else
            {
                baseScore = 40;
            }


            // Give additional weight to actual
            // WorkExperience records.

            if (workExperiences.Any())
            {
                int relevantExperiences = 0;

                foreach (var experience
                    in workExperiences)
                {
                    string experienceText =
                        $"{experience.JobTitle} " +
                        $"{experience.CompanyName} " +
                        $"{experience.Description}";

                    if (ContainsRelatedTerm(
                        jobText,
                        NormalizeText(
                            experienceText)))
                    {
                        relevantExperiences++;
                    }
                }

                if (relevantExperiences >= 2)
                {
                    baseScore += 10;
                }
                else if (relevantExperiences == 1)
                {
                    baseScore += 5;
                }
            }

            return Math.Clamp(
                baseScore,
                0,
                100);
        }


        // ======================================================
        // EDUCATION SCORE
        // ======================================================

        private static decimal CalculateEducationScore(
            string jobText,
            List<Education> educations)
        {
            if (!educations.Any())
            {
                return 40;
            }

            foreach (var education in educations)
            {
                string educationText =
                    $"{education.Degree} " +
                    $"{education.FieldOfStudy} " +
                    $"{education.Description}";

                string normalizedEducation =
                    NormalizeText(educationText);

                var educationTerms =
                    normalizedEducation
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length >= 5)
                        .Distinct()
                        .ToList();

                int matches = educationTerms.Count(
                    term =>
                        jobText.Contains(
                            term,
                            StringComparison.OrdinalIgnoreCase));

                if (matches >= 3)
                {
                    return 100;
                }

                if (matches >= 1)
                {
                    return 80;
                }
            }

            return 60;
        }


        // ======================================================
        // PROJECT SCORE
        // ======================================================

        private static decimal CalculateProjectScore(
            string jobText,
            List<Project> projects)
        {
            if (!projects.Any())
            {
                return 40;
            }

            int relevantProjects = 0;

            foreach (var project in projects)
            {
                string projectText =
                    $"{project.ProjectName} " +
                    $"{project.Description} " +
                    $"{project.Technologies}";

                if (ContainsRelatedTerm(
                    jobText,
                    NormalizeText(projectText)))
                {
                    relevantProjects++;
                }
            }

            if (relevantProjects >= 3)
            {
                return 100;
            }

            if (relevantProjects == 2)
            {
                return 90;
            }

            if (relevantProjects == 1)
            {
                return 75;
            }

            return 50;
        }


        // ======================================================
        // RELATED TERM CHECK
        // ======================================================

        private static bool ContainsRelatedTerm(
            string jobText,
            string candidateText)
        {
            if (string.IsNullOrWhiteSpace(jobText) ||
                string.IsNullOrWhiteSpace(candidateText))
            {
                return false;
            }

            var jobWords =
                jobText
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length >= 4)
                    .Distinct()
                    .ToList();

            int matches = 0;

            foreach (var word in jobWords)
            {
                if (candidateText.Contains(
                    word,
                    StringComparison.OrdinalIgnoreCase))
                {
                    matches++;

                    if (matches >= 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}