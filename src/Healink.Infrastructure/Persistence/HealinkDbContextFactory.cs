using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Healink.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for HealinkDbContext to support EF Core tools
/// </summary>
public class HealinkDbContextFactory : IDesignTimeDbContextFactory<HealinkDbContext>
{
    public HealinkDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HealinkDbContext>();
        
        // Use hardcoded connection string for design-time
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=HealinkDb;Trusted_Connection=true;MultipleActiveResultSets=true";
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new HealinkDbContext(optionsBuilder.Options);
    }
}
