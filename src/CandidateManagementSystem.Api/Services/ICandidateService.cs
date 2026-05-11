using CandidateManagementSystem.Api.DTOs.Candidate;

namespace CandidateManagementSystem.Api.Services;

public interface ICandidateService
{
    Task<IReadOnlyList<CandidateResponse>> SearchAsync(CandidateSearchRequest request, CancellationToken ct = default);
    Task<CandidateResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CandidateResponse> CreateAsync(CreateCandidateRequest request, CancellationToken ct = default);
    Task<CandidateResponse?> UpdateAsync(int id, UpdateCandidateRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<CandidateResponse?> AddSkillAsync(int candidateId, int skillId, CancellationToken ct = default);
    Task<CandidateResponse?> RemoveSkillAsync(int candidateId, int skillId, CancellationToken ct = default);
}
