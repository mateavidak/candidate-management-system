using CandidateManagementSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var usePostgres = string.Equals(
            Database.ProviderName,
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            StringComparison.Ordinal);

        if (usePostgres)
            modelBuilder.HasPostgresExtension("citext");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        if (!usePostgres)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var check in entityType.GetDeclaredCheckConstraints().ToList())
                {
                    if (!string.IsNullOrEmpty(check.Name))
                        entityType.RemoveCheckConstraint(check.Name);
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (string.Equals(property.GetColumnType(), "citext", StringComparison.OrdinalIgnoreCase))
                        property.SetColumnType("TEXT");
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
