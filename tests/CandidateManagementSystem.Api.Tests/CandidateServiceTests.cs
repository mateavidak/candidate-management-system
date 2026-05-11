using CandidateManagementSystem.Api.DTOs.Candidate;
using CandidateManagementSystem.Api.Entities;
using CandidateManagementSystem.Api.Exceptions;
using CandidateManagementSystem.Api.Services;
using CandidateManagementSystem.Api.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Tests;

public class CandidateServiceTests
{
    [Fact]
    public async Task SearchAsync_by_skill_ids_returns_only_candidates_with_all_skills()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var java = new Skill { Name = "Java programming" };
        var csharp = new Skill { Name = "C# programming" };
        db.Skills.AddRange(java, csharp);
        await db.SaveChangesAsync();

        var alice = new Candidate
        {
            FullName = "Alice Example",
            DateOfBirth = new DateOnly(1990, 1, 1),
            ContactNumber = "+1234567890",
            Email = "alice@example.com",
        };
        alice.Skills.Add(java);
        alice.Skills.Add(csharp);

        var bob = new Candidate
        {
            FullName = "Bob Example",
            DateOfBirth = new DateOnly(1991, 2, 2),
            ContactNumber = "+1234567891",
            Email = "bob@example.com",
        };
        bob.Skills.Add(java);

        db.Candidates.AddRange(alice, bob);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest(null, [java.Id, csharp.Id], null));

        Assert.Single(results);
        Assert.Equal("Alice Example", results[0].FullName);
    }

    [Fact]
    public async Task SearchAsync_unknown_skill_name_returns_empty()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Skills.Add(new Skill { Name = "English" });
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest(null, null, ["Klingon"]));

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_by_name_is_case_insensitive()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Candidates.Add(new Candidate
        {
            FullName = "Marija Petrović",
            DateOfBirth = new DateOnly(1991, 4, 4),
            ContactNumber = "+1234567899",
            Email = "marija@example.com",
        });
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest("PETROVIĆ", null, null));

        Assert.Single(results);
        Assert.Equal("marija@example.com", results[0].Email);
    }

    [Fact]
    public async Task SearchAsync_by_name_and_skill_names_combined()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var english = new Skill { Name = "English" };
        db.Skills.Add(english);
        await db.SaveChangesAsync();

        var candidate = new Candidate
        {
            FullName = "Ana Marković",
            DateOfBirth = new DateOnly(1992, 2, 2),
            ContactNumber = "+1234567891",
            Email = "ana@example.com",
        };
        candidate.Skills.Add(english);
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest("marković", null, ["English"]));

        Assert.Single(results);
        Assert.Equal("ana@example.com", results[0].Email);
    }

    [Fact]
    public async Task SearchAsync_by_multiple_skill_names_requires_all_skills()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sql = new Skill { Name = "SQL" };
        var csharp = new Skill { Name = "C# programming" };
        db.Skills.AddRange(sql, csharp);
        await db.SaveChangesAsync();

        var fullMatch = new Candidate
        {
            FullName = "Full Stack Dev",
            DateOfBirth = new DateOnly(1993, 5, 5),
            ContactNumber = "+1234567892",
            Email = "full@example.com",
        };
        fullMatch.Skills.Add(sql);
        fullMatch.Skills.Add(csharp);

        var partial = new Candidate
        {
            FullName = "Backend Only",
            DateOfBirth = new DateOnly(1994, 6, 6),
            ContactNumber = "+1234567893",
            Email = "back@example.com",
        };
        partial.Skills.Add(sql);

        db.Candidates.AddRange(fullMatch, partial);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest(null, null, ["SQL", "C# programming"]));

        Assert.Single(results);
        Assert.Equal("full@example.com", results[0].Email);
    }

    [Fact]
    public async Task CreateAsync_duplicate_email_throws_conflict()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Candidates.Add(new Candidate
        {
            FullName = "Existing User",
            DateOfBirth = new DateOnly(1980, 1, 1),
            ContactNumber = "+1234567893",
            Email = "dup@example.com",
        });
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var ex = await Assert.ThrowsAsync<ApiConflictException>(() =>
            sut.CreateAsync(new CreateCandidateRequest(
                "Other Name",
                new DateOnly(1990, 1, 1),
                "+1234567894",
                "dup@example.com")));

        Assert.Equal("DuplicateEmail", ex.Code);
    }

    [Fact]
    public async Task UpdateAsync_duplicate_email_throws_conflict()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var a = new Candidate
        {
            FullName = "A",
            DateOfBirth = new DateOnly(1980, 1, 1),
            ContactNumber = "+1111111111",
            Email = "a@example.com",
        };
        var b = new Candidate
        {
            FullName = "B",
            DateOfBirth = new DateOnly(1981, 1, 1),
            ContactNumber = "+2222222222",
            Email = "b@example.com",
        };
        db.Candidates.AddRange(a, b);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var ex = await Assert.ThrowsAsync<ApiConflictException>(() =>
            sut.UpdateAsync(a.Id, new UpdateCandidateRequest(
                "A updated",
                new DateOnly(1980, 1, 1),
                "+1111111111",
                "b@example.com")));

        Assert.Equal("DuplicateEmail", ex.Code);
    }

    [Fact]
    public async Task AddSkillAsync_when_already_assigned_throws_conflict()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var skill = new Skill { Name = "SQL" };
        db.Skills.Add(skill);
        var candidate = new Candidate
        {
            FullName = "Dev",
            DateOfBirth = new DateOnly(1992, 1, 1),
            ContactNumber = "+1234567895",
            Email = "dev@example.com",
        };
        candidate.Skills.Add(skill);
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var ex = await Assert.ThrowsAsync<ApiConflictException>(() =>
            sut.AddSkillAsync(candidate.Id, skill.Id));

        Assert.Equal("SkillAlreadyAssigned", ex.Code);
    }

    [Fact]
    public async Task SearchAsync_empty_filters_returns_all_ordered_by_full_name()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Candidates.AddRange(
            new Candidate
            {
                FullName = "Zoe Zed",
                DateOfBirth = new DateOnly(1990, 1, 1),
                ContactNumber = "+1000000001",
                Email = "z@example.com",
            },
            new Candidate
            {
                FullName = "Adam A",
                DateOfBirth = new DateOnly(1991, 2, 2),
                ContactNumber = "+1000000002",
                Email = "a@example.com",
            });
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest(null, null, null));

        Assert.Equal(2, results.Count);
        Assert.Equal(new[] { "Adam A", "Zoe Zed" }, results.Select(r => r.FullName).ToArray());
    }

    [Fact]
    public async Task SearchAsync_merges_skill_ids_and_skill_names_into_single_and_filter()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var java = new Skill { Name = "Java programming" };
        var english = new Skill { Name = "English" };
        db.Skills.AddRange(java, english);
        await db.SaveChangesAsync();

        var candidate = new Candidate
        {
            FullName = "Polyglot",
            DateOfBirth = new DateOnly(1995, 3, 3),
            ContactNumber = "+1000000003",
            Email = "poly@example.com",
        };
        candidate.Skills.Add(java);
        candidate.Skills.Add(english);
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var results = await sut.SearchAsync(new CandidateSearchRequest(null, [java.Id], ["English"]));

        Assert.Single(results);
        Assert.Equal("poly@example.com", results[0].Email);
    }

    [Fact]
    public async Task GetByIdAsync_returns_candidate_with_skills()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var skill = new Skill { Name = "Git" };
        db.Skills.Add(skill);
        var candidate = new Candidate
        {
            FullName = "Tester",
            DateOfBirth = new DateOnly(1993, 4, 4),
            ContactNumber = "+1000000004",
            Email = "tester@example.com",
        };
        candidate.Skills.Add(skill);
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.GetByIdAsync(candidate.Id);

        Assert.NotNull(result);
        Assert.Equal("Tester", result.FullName);
        Assert.Single(result.Skills);
        Assert.Equal("Git", result.Skills[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new CandidateService(db);

        var result = await sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_persists_and_returns_response()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new CandidateService(db);

        var result = await sut.CreateAsync(new CreateCandidateRequest(
            "  New Person  ",
            new DateOnly(1988, 7, 7),
            "+1000000005",
            "  new.person@example.com  "));

        Assert.True(result.Id > 0);
        Assert.Equal("New Person", result.FullName);
        Assert.Equal("new.person@example.com", result.Email);
        Assert.Empty(result.Skills);
        Assert.True(await db.Candidates.AnyAsync(c => c.Id == result.Id));
    }

    [Fact]
    public async Task UpdateAsync_updates_fields_when_found()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var candidate = new Candidate
        {
            FullName = "Old",
            DateOfBirth = new DateOnly(1990, 1, 1),
            ContactNumber = "+1000000006",
            Email = "old@example.com",
        };
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.UpdateAsync(candidate.Id, new UpdateCandidateRequest(
            "New Name",
            new DateOnly(1991, 2, 2),
            "+1000000007",
            "new@example.com"));

        Assert.NotNull(result);
        Assert.Equal("New Name", result!.FullName);
        Assert.Equal(new DateOnly(1991, 2, 2), result.DateOfBirth);
        Assert.Equal("+1000000007", result.ContactNumber);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_allows_keeping_same_email_for_same_candidate()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var candidate = new Candidate
        {
            FullName = "Same Email User",
            DateOfBirth = new DateOnly(1985, 5, 5),
            ContactNumber = "+1000000014",
            Email = "same@example.com",
        };
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.UpdateAsync(candidate.Id, new UpdateCandidateRequest(
            "Updated Name",
            new DateOnly(1985, 6, 6),
            "+1000000015",
            "same@example.com"));

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result!.FullName);
        Assert.Equal("same@example.com", result.Email);
    }

    [Fact]
    public async Task UpdateAsync_returns_null_when_not_found()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new CandidateService(db);

        var result = await sut.UpdateAsync(404, new UpdateCandidateRequest(
            "X",
            new DateOnly(1990, 1, 1),
            "+1000000008",
            "x@example.com"));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_returns_true_and_removes_candidate()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var candidate = new Candidate
        {
            FullName = "To Delete",
            DateOfBirth = new DateOnly(1992, 1, 1),
            ContactNumber = "+1000000009",
            Email = "del@example.com",
        };
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var deleted = await sut.DeleteAsync(candidate.Id);

        Assert.True(deleted);
        Assert.Null(await sut.GetByIdAsync(candidate.Id));
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new CandidateService(db);

        var deleted = await sut.DeleteAsync(999);

        Assert.False(deleted);
    }

    [Fact]
    public async Task AddSkillAsync_adds_skill_when_valid()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var skill = new Skill { Name = "Docker" };
        db.Skills.Add(skill);
        var candidate = new Candidate
        {
            FullName = "Dev",
            DateOfBirth = new DateOnly(1994, 1, 1),
            ContactNumber = "+1000000010",
            Email = "dev2@example.com",
        };
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.AddSkillAsync(candidate.Id, skill.Id);

        Assert.NotNull(result);
        Assert.Single(result!.Skills);
        Assert.Equal("Docker", result.Skills[0].Name);
    }

    [Fact]
    public async Task AddSkillAsync_returns_null_when_candidate_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var skill = new Skill { Name = "Kotlin programming" };
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.AddSkillAsync(999, skill.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddSkillAsync_returns_null_when_skill_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var candidate = new Candidate
        {
            FullName = "Solo",
            DateOfBirth = new DateOnly(1996, 1, 1),
            ContactNumber = "+1000000011",
            Email = "solo@example.com",
        };
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.AddSkillAsync(candidate.Id, 999);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveSkillAsync_removes_skill_when_present()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var s1 = new Skill { Name = "A" };
        var s2 = new Skill { Name = "B" };
        db.Skills.AddRange(s1, s2);
        var candidate = new Candidate
        {
            FullName = "Has Two",
            DateOfBirth = new DateOnly(1997, 1, 1),
            ContactNumber = "+1000000012",
            Email = "two@example.com",
        };
        candidate.Skills.Add(s1);
        candidate.Skills.Add(s2);
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.RemoveSkillAsync(candidate.Id, s1.Id);

        Assert.NotNull(result);
        Assert.Single(result!.Skills);
        Assert.Equal("B", result.Skills[0].Name);
    }

    [Fact]
    public async Task RemoveSkillAsync_returns_null_when_candidate_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var skill = new Skill { Name = "X" };
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.RemoveSkillAsync(999, skill.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveSkillAsync_when_skill_not_linked_returns_response_unchanged()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var linked = new Skill { Name = "Linked" };
        var other = new Skill { Name = "Other" };
        db.Skills.AddRange(linked, other);
        var candidate = new Candidate
        {
            FullName = "One Skill",
            DateOfBirth = new DateOnly(1998, 1, 1),
            ContactNumber = "+1000000013",
            Email = "one@example.com",
        };
        candidate.Skills.Add(linked);
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();

        var sut = new CandidateService(db);
        var result = await sut.RemoveSkillAsync(candidate.Id, other.Id);

        Assert.NotNull(result);
        Assert.Single(result!.Skills);
        Assert.Equal("Linked", result.Skills[0].Name);
    }
}
