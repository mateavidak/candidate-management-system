using CandidateManagementSystem.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CandidateManagementSystem.Api.Tests.Support;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
