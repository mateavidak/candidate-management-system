namespace CandidateManagementSystem.Api.DTOs.Candidate;

public record CandidateSearchRequest(
    string? Name,
    IReadOnlyList<int>? SkillIds,
    IReadOnlyList<string>? SkillNames
);
