using CandidateManagementSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Persistence;

public static class DbSeeder
{
    public static async Task EnsureSeedDataAsync(ApplicationDbContext db, CancellationToken ct = default)
    {
        var skillNames = new[]
        {
            "Java programming",
            "C# programming",
            "Python programming",
            "JavaScript programming",
            "TypeScript programming",
            "Go programming",
            "Rust programming",
            "Kotlin programming",
            "C++ programming",
            "Ruby programming",
            "PHP programming",
            "Swift programming",
            "SQL",
            "Database design",
            "REST API design",
            "Git",
            "Docker",
            "English",
            "Russian",
            "German",
            "French",
            "Spanish",
            "Italian",
            "Portuguese",
            "Polish",
            "Croatian",
            "Serbian",
            "Dutch",
            "Swedish",
            "Turkish",
            "Greek",
            "Hungarian",
        };

        foreach (var name in skillNames)
        {
            var exists = await db.Skills.AnyAsync(s => s.Name == name, ct);
            if (!exists)
                db.Skills.Add(new Skill { Name = name });
        }

        await db.SaveChangesAsync(ct);

        var skillsByName = await db.Skills
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Name, s => s.Id, StringComparer.OrdinalIgnoreCase, ct);

        var seedCandidates = new[]
        {
            (
                FullName: "Ana Marković",
                DateOfBirth: new DateOnly(1992, 3, 18),
                ContactNumber: "+38761111222",
                Email: "ana.markovic@example.com",
                SkillNames: new[] { "C# programming", "Database design", "English", "TypeScript programming", "Git" }
            ),
            (
                FullName: "Marko Petrović",
                DateOfBirth: new DateOnly(1988, 11, 2),
                ContactNumber: "+38762222333",
                Email: "marko.petrovic@example.com",
                SkillNames: new[] { "Java programming", "Python programming", "English", "German", "Docker" }
            ),
            (
                FullName: "Jelena Kostić",
                DateOfBirth: new DateOnly(1995, 7, 25),
                ContactNumber: "+38763333444",
                Email: "jelena.kostic@example.com",
                SkillNames: new[] { "C# programming", "Russian", "SQL", "REST API design", "Croatian" }
            ),
        };

        foreach (var row in seedCandidates)
        {
            var email = row.Email.Trim();
            if (await db.Candidates.AnyAsync(c => c.Email == email, ct))
                continue;

            var candidate = new Candidate
            {
                FullName = row.FullName,
                DateOfBirth = row.DateOfBirth,
                ContactNumber = row.ContactNumber,
                Email = email,
            };

            foreach (var skillName in row.SkillNames)
            {
                if (!skillsByName.TryGetValue(skillName, out var skillId))
                    continue;

                var skill = await db.Skills.FindAsync([skillId], ct);
                if (skill is not null)
                    candidate.Skills.Add(skill);
            }

            db.Candidates.Add(candidate);
        }

        await db.SaveChangesAsync(ct);
    }
}
