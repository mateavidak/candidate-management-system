using CandidateManagementSystem.Api.DTOs.Skill;

namespace CandidateManagementSystem.Api.DTOs.Candidate;

public record CandidateResponse(
    int Id,
    string FullName,
    DateOnly DateOfBirth,
    string ContactNumber,
    string Email,
    IReadOnlyList<SkillResponse> Skills
);
