using CandidateManagementSystem.Api.DTOs.Candidate;
using CandidateManagementSystem.Api.DTOs.Skill;
using CandidateManagementSystem.Api.Entities;
using CandidateManagementSystem.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Services;

public class CandidateService(ApplicationDbContext db) : ICandidateService
{
    public async Task<IReadOnlyList<CandidateResponse>> SearchAsync(CandidateSearchRequest request, CancellationToken ct = default)
    {
        var requiredSkillIds = new HashSet<int>();

        if (request.SkillIds is { Count: > 0 })
        {
            foreach (var id in request.SkillIds)
                requiredSkillIds.Add(id);
        }

        var distinctSkillNames = (request.SkillNames ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctSkillNames.Count > 0)
        {
            var nameArray = distinctSkillNames.ToArray();
            var matchedIds = await db.Skills.AsNoTracking()
                .Where(s => nameArray.Contains(s.Name))
                .Select(s => s.Id)
                .Distinct()
                .ToListAsync(ct);

            if (matchedIds.Count != distinctSkillNames.Count)
                return [];

            foreach (var id in matchedIds)
                requiredSkillIds.Add(id);
        }

        var query = db.Candidates.Include(c => c.Skills).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var term = request.Name.Trim().Replace("%", string.Empty).Replace("_", string.Empty);
            if (term.Length > 0)
                query = query.Where(c => EF.Functions.ILike(c.FullName, $"%{term}%"));
        }

        foreach (var skillId in requiredSkillIds)
            query = query.Where(c => c.Skills.Any(s => s.Id == skillId));

        return await query
            .OrderBy(c => c.FullName)
            .Select(c => ToResponse(c))
            .ToListAsync(ct);
    }

    public async Task<CandidateResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var candidate = await db.Candidates
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return candidate is null ? null : ToResponse(candidate);
    }

    public async Task<CandidateResponse> CreateAsync(CreateCandidateRequest request, CancellationToken ct = default)
    {
        var candidate = new Candidate
        {
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            ContactNumber = request.ContactNumber.Trim(),
            Email = request.Email.Trim()
        };

        db.Candidates.Add(candidate);
        await db.SaveChangesAsync(ct);
        return ToResponse(candidate);
    }

    public async Task<CandidateResponse?> UpdateAsync(int id, UpdateCandidateRequest request, CancellationToken ct = default)
    {
        var candidate = await db.Candidates
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (candidate is null) return null;

        candidate.FullName = request.FullName.Trim();
        candidate.DateOfBirth = request.DateOfBirth;
        candidate.ContactNumber = request.ContactNumber.Trim();
        candidate.Email = request.Email.Trim();

        await db.SaveChangesAsync(ct);
        return ToResponse(candidate);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var candidate = await db.Candidates.FindAsync([id], ct);
        if (candidate is null) return false;

        db.Candidates.Remove(candidate);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CandidateResponse?> AddSkillAsync(int candidateId, int skillId, CancellationToken ct = default)
    {
        var candidate = await db.Candidates
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == candidateId, ct);

        if (candidate is null) return null;

        if (candidate.Skills.Any(s => s.Id == skillId))
            return ToResponse(candidate);

        var skill = await db.Skills.FindAsync([skillId], ct);
        if (skill is null) return null;

        candidate.Skills.Add(skill);
        await db.SaveChangesAsync(ct);
        return ToResponse(candidate);
    }

    public async Task<CandidateResponse?> RemoveSkillAsync(int candidateId, int skillId, CancellationToken ct = default)
    {
        var candidate = await db.Candidates
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == candidateId, ct);

        if (candidate is null) return null;

        var skill = candidate.Skills.FirstOrDefault(s => s.Id == skillId);
        if (skill is null) return ToResponse(candidate);

        candidate.Skills.Remove(skill);
        await db.SaveChangesAsync(ct);
        return ToResponse(candidate);
    }

    private static CandidateResponse ToResponse(Candidate c) =>
        new(
            c.Id,
            c.FullName,
            c.DateOfBirth,
            c.ContactNumber,
            c.Email,
            c.Skills.Select(s => new SkillResponse(s.Id, s.Name)).ToList()
        );
}
