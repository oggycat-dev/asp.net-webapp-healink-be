using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Healink.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for ApplicationDbContext
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use a default connection string for design-time
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HealinkDb;Trusted_Connection=true;MultipleActiveResultSets=true");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
} 