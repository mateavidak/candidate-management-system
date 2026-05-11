using CandidateManagementSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
