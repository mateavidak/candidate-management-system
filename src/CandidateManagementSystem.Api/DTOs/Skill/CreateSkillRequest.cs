using System.ComponentModel.DataAnnotations;

namespace CandidateManagementSystem.Api.DTOs.Skill;

public record CreateSkillRequest(
    [Required, MaxLength(200)] string Name
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length < 2)
            yield return new ValidationResult("Skill name must be at least 2 characters (not only spaces).", [nameof(Name)]);
    }
}
