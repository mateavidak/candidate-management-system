namespace CandidateManagementSystem.Api.Exceptions;

public sealed class ApiConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
