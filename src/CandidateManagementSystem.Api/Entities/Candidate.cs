namespace CandidateManagementSystem.Api.Entities;

public class Candidate
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
    public string ContactNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
