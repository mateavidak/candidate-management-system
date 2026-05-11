namespace CandidateManagementSystem.Api.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
}
