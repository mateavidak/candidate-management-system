using System.ComponentModel.DataAnnotations;

namespace CandidateManagementSystem.Api.DTOs.Skill;

public record CreateSkillRequest(
    [Required, MaxLength(200)] string Name
);
