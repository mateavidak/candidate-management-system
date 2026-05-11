using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CandidateManagementSystem.Api.DTOs.Candidate;

public record UpdateCandidateRequest(
    [Required, MaxLength(200)] string FullName,
    [Required] DateOnly DateOfBirth,
    [Required, MaxLength(50)] string ContactNumber,
    [Required, EmailAddress, MaxLength(320)] string Email
) : IValidatableObject
{
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9][0-9\s\-().]{5,49}$", RegexOptions.Compiled);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FullName) || FullName.Trim().Length < 2)
            yield return new ValidationResult("Full name must be at least 2 characters (not only spaces).", [nameof(FullName)]);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (DateOfBirth > today)
            yield return new ValidationResult("Date of birth cannot be in the future.", [nameof(DateOfBirth)]);

        if (string.IsNullOrWhiteSpace(ContactNumber) || ContactNumber.Trim().Length < 6)
            yield return new ValidationResult("Contact number must be at least 6 characters.", [nameof(ContactNumber)]);
        else if (!PhoneRegex.IsMatch(ContactNumber.Trim()))
            yield return new ValidationResult("Contact number has an invalid format.", [nameof(ContactNumber)]);

        if (string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult("Email is required.", [nameof(Email)]);
    }
}
