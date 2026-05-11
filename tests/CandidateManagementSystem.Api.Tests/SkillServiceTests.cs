using CandidateManagementSystem.Api.DTOs.Skill;
using CandidateManagementSystem.Api.Entities;
using CandidateManagementSystem.Api.Exceptions;
using CandidateManagementSystem.Api.Services;
using CandidateManagementSystem.Api.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Tests;

public class SkillServiceTests
{
    [Fact]
    public async Task CreateAsync_inserts_new_skill()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new SkillService(db);

        var created = await sut.CreateAsync(new CreateSkillRequest("Rust programming"));

        Assert.True(created.Id > 0);
        Assert.Equal("Rust programming", created.Name);
        Assert.True(await db.Skills.AnyAsync(s => s.Name == "Rust programming"));
    }

    [Fact]
    public async Task CreateAsync_duplicate_name_throws_conflict()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Skills.Add(new Skill { Name = "Go programming" });
        await db.SaveChangesAsync();

        var sut = new SkillService(db);

        var ex = await Assert.ThrowsAsync<ApiConflictException>(() =>
            sut.CreateAsync(new CreateSkillRequest("Go programming")));

        Assert.Equal("DuplicateSkillName", ex.Code);
    }

    [Fact]
    public async Task GetAllAsync_orders_by_name()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Skills.AddRange(
            new Skill { Name = "Zebra skill" },
            new Skill { Name = "Alpha skill" });
        await db.SaveChangesAsync();

        var sut = new SkillService(db);
        var list = await sut.GetAllAsync();

        Assert.Equal(new[] { "Alpha skill", "Zebra skill" }, list.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task GetByIdAsync_returns_skill_when_found()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Skills.Add(new Skill { Name = "TypeScript programming" });
        await db.SaveChangesAsync();
        var id = db.Skills.Single().Id;

        var sut = new SkillService(db);
        var result = await sut.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.Equal("TypeScript programming", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new SkillService(db);

        var result = await sut.GetByIdAsync(42);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_returns_true_and_removes_skill()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        db.Skills.Add(new Skill { Name = "To remove" });
        await db.SaveChangesAsync();
        var id = db.Skills.Single().Id;

        var sut = new SkillService(db);
        var deleted = await sut.DeleteAsync(id);

        Assert.True(deleted);
        Assert.Null(await sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_missing()
    {
        await using var db = TestDbContextFactory.CreateInMemory();
        var sut = new SkillService(db);

        var deleted = await sut.DeleteAsync(999);

        Assert.False(deleted);
    }
}
