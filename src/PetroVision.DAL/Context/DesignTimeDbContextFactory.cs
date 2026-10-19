using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetroVision.DAL.Context;

// Used only by "dotnet ef" commands (migrations) so they don't need to run the API.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PetroVisionDbContext>
{
    public PetroVisionDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PetroVisionDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PetroVisionDb;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new PetroVisionDbContext(options);
    }
}
