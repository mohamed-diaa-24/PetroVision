using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetroVision.DAL.Context;

// Used only by "dotnet ef" commands (migrations) so they don't need to run the API.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PetroVisionDbContext>
{
    public PetroVisionDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PetroVisionDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=PetroVision;User Id=sa;Password=P@ssword12345!;TrustServerCertificate=True;")
            .Options;

        return new PetroVisionDbContext(options);
    }
}
