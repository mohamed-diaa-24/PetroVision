using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PetroVision.DAL.Context;
using PetroVision.DAL.Seed;

namespace PetroVision.DAL;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PetroVisionDbContext>(o => o.UseSqlServer(connectionString));
        return services;
    }

    public static async Task InitializeDataAccessAsync(this IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroVisionDbContext>();

        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
    }
}
