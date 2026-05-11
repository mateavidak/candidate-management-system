using CandidateManagementSystem.Api.DTOs.Skill;
using CandidateManagementSystem.Api.Entities;
using CandidateManagementSystem.Api.Exceptions;
using CandidateManagementSystem.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Services;

public class SkillService(ApplicationDbContext db) : ISkillService
{
    public async Task<IReadOnlyList<SkillResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Skills
            .OrderBy(s => s.Name)
            .Select(s => new SkillResponse(s.Id, s.Name))
            .ToListAsync(ct);
    }

    public async Task<SkillResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var skill = await db.Skills.FindAsync([id], ct);
        return skill is null ? null : new SkillResponse(skill.Id, skill.Name);
    }

    public async Task<SkillResponse> CreateAsync(CreateSkillRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await db.Skills.AnyAsync(s => s.Name == name, ct))
            throw new ApiConflictException("DuplicateSkillName", "A skill with this name already exists.");

        var skill = new Skill { Name = name };
        db.Skills.Add(skill);
        await db.SaveChangesAsync(ct);
        return new SkillResponse(skill.Id, skill.Name);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var skill = await db.Skills.FindAsync([id], ct);
        if (skill is null) return false;

        db.Skills.Remove(skill);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
