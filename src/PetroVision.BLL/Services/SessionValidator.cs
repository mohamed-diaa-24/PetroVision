using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class SessionValidator : ISessionValidator
{
    private static readonly TimeSpan ActivityResolution = TimeSpan.FromMinutes(1);

    private readonly PetroVisionDbContext _db;

    public SessionValidator(PetroVisionDbContext db) => _db = db;

    public async Task<bool> ValidateAsync(int userId, string? securityStamp)
    {
        if (string.IsNullOrEmpty(securityStamp)) return false;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        // Ended service, rejected account, changed permissions or password -> token is dead.
        if (user is null || user.Status != UserStatus.Active || user.SecurityStamp != securityStamp)
            return false;

        // Keeps "online / last active" accurate without writing on every request.
        var now = DateTime.UtcNow;
        if (user.LastActiveAt is null || now - user.LastActiveAt.Value > ActivityResolution)
        {
            user.LastActiveAt = now;
            await _db.SaveChangesAsync();
        }

        return true;
    }
}
