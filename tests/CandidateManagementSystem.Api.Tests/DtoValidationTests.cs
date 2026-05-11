using System.ComponentModel.DataAnnotations;
using CandidateManagementSystem.Api.DTOs.Candidate;
using CandidateManagementSystem.Api.DTOs.Skill;

namespace CandidateManagementSystem.Api.Tests;

public class DtoValidationTests
{
    private static void AssertInvalid(object instance, params string[] expectedPropertyNames)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        if (instance is IValidatableObject validatable)
            results.AddRange(validatable.Validate(context));

        Assert.NotEmpty(results);
        foreach (var name in expectedPropertyNames)
            Assert.Contains(results, r => r.MemberNames.Contains(name));
    }

    [Fact]
    public void CreateCandidateRequest_future_date_of_birth_invalid()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var request = new CreateCandidateRequest(
            "Valid Name",
            tomorrow,
            "+1234567890",
            "ok@example.com");

        AssertInvalid(request, nameof(CreateCandidateRequest.DateOfBirth));
    }

    [Fact]
    public void CreateCandidateRequest_short_full_name_invalid()
    {
        var request = new CreateCandidateRequest(
            " X ",
            new DateOnly(1990, 1, 1),
            "+1234567890",
            "ok@example.com");

        AssertInvalid(request, nameof(CreateCandidateRequest.FullName));
    }

    [Fact]
    public void CreateCandidateRequest_invalid_phone_invalid()
    {
        var request = new CreateCandidateRequest(
            "Valid Person",
            new DateOnly(1990, 1, 1),
            "abc-not-a-phone",
            "ok@example.com");

        AssertInvalid(request, nameof(CreateCandidateRequest.ContactNumber));
    }

    [Fact]
    public void CreateSkillRequest_whitespace_name_invalid()
    {
        var request = new CreateSkillRequest("  a  ");

        AssertInvalid(request, nameof(CreateSkillRequest.Name));
    }

    [Fact]
    public void UpdateCandidateRequest_same_rules_as_create()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var request = new UpdateCandidateRequest(
            "Valid Name",
            tomorrow,
            "+9876543210",
            "x@example.com");

        AssertInvalid(request, nameof(UpdateCandidateRequest.DateOfBirth));
    }
}
