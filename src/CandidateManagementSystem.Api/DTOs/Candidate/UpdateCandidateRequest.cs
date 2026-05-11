using System.ComponentModel.DataAnnotations;

namespace CandidateManagementSystem.Api.DTOs.Candidate;

public record UpdateCandidateRequest(
    [Required, MaxLength(200)] string FullName,
    [Required] DateOnly DateOfBirth,
    [Required, MaxLength(50)] string ContactNumber,
    [Required, EmailAddress, MaxLength(320)] string Email
);
