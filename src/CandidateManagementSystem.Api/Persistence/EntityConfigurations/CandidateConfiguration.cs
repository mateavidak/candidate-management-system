using CandidateManagementSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CandidateManagementSystem.Api.Persistence.EntityConfigurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("Candidates");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.DateOfBirth)
            .IsRequired();

        builder.Property(c => c.ContactNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(320)
            .HasColumnType("citext");

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.HasMany(c => c.Skills)
            .WithMany(s => s.Candidates)
            .UsingEntity<Dictionary<string, object>>(
                "CandidateSkills",
                j => j.HasOne<Skill>().WithMany().HasForeignKey("SkillId"),
                j => j.HasOne<Candidate>().WithMany().HasForeignKey("CandidateId"),
                j =>
                {
                    j.ToTable("CandidateSkills");
                    j.HasKey("CandidateId", "SkillId");
                });
    }
}
