using CandidateManagementSystem.Api.DTOs.Skill;

namespace CandidateManagementSystem.Api.Services;

public interface ISkillService
{
    Task<IReadOnlyList<SkillResponse>> GetAllAsync(CancellationToken ct = default);
    Task<SkillResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SkillResponse> CreateAsync(CreateSkillRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
