using Microsoft.EntityFrameworkCore;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(PetroVisionDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        var now = DateTime.UtcNow;

        // ---------- Users ----------
        var manager = new User { FullName = "Mohamed S.", EmployeeCode = "1001", Email = "manager@petrovision.com", PasswordHash = Hash("Manager@123"), Role = UserRole.Manager, Status = UserStatus.Active, LastActiveAt = now };
        var owner   = new User { FullName = "Owner", EmployeeCode = "1002", Email = "owner@petrovision.com", PasswordHash = Hash("Owner@123"), Role = UserRole.Owner, Status = UserStatus.Active };
        var ahmed   = new User { FullName = "Ahmed S.", EmployeeCode = "4710", Email = "ahmed@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Active, LastActiveAt = now.AddMinutes(-5) };
        var mona    = new User { FullName = "Mona K.", EmployeeCode = "4821", Email = "mona@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Active, LastActiveAt = now.AddHours(-4) };
        var omar    = new User { FullName = "Omar H.", EmployeeCode = "4833", Email = "omar@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Expired };
        var khaled  = new User { FullName = "Khaled R.", EmployeeCode = "4850", Email = "khaled@petrovision.com", PasswordHash = Hash("Agent@123"), Role = UserRole.Agent, Status = UserStatus.Pending };

        db.Users.AddRange(manager, owner, ahmed, mona, omar, khaled);

        db.AgentPermissions.AddRange(
            new AgentPermission { User = ahmed, Permission = PermissionType.ReassignIncidents },
            new AgentPermission { User = ahmed, Permission = PermissionType.PostAnnouncements },
            new AgentPermission { User = mona, Permission = PermissionType.ManageCameras });

        // ---------- Cameras ----------
        var cam1 = NewCamera("Cam 1", "Gate A", CameraHealth.Online, now, DetectionType.PPE);
        var cam2 = NewCamera("Cam 2", "Zone A", CameraHealth.Online, now, DetectionType.PPE, DetectionType.Fall);
        var cam3 = NewCamera("Cam 3", "Zone B", CameraHealth.Online, now.AddMinutes(-1), DetectionType.PPE, DetectionType.Fall);
        var cam4 = NewCamera("Cam 4", "Yard", CameraHealth.Offline, now.AddMinutes(-50), DetectionType.PPE);
        var cam5 = NewCamera("Cam 5", "Zone C", CameraHealth.Online, now, DetectionType.Fire);

        db.Cameras.AddRange(cam1, cam2, cam3, cam4, cam5);

        // ---------- Alerts ----------
        db.Alerts.AddRange(
            new Alert { Camera = cam2, Type = DetectionType.PPE, Level = AlertLevel.Medium, Status = AlertStatus.New, Reason = "Missing safety helmet", Confidence = 0.81, CreatedAt = now.AddMinutes(-3) },
            new Alert { Camera = cam1, Type = DetectionType.PPE, Level = AlertLevel.Low, Status = AlertStatus.Acknowledged, Reason = "PPE check", Confidence = 0.65, CreatedAt = now.AddMinutes(-18), HandledBy = ahmed, HandledAt = now.AddMinutes(-15) },
            new Alert { Camera = cam1, Type = DetectionType.PPE, Level = AlertLevel.Low, Status = AlertStatus.Dismissed, Reason = "Wrong detection", Confidence = 0.61, CreatedAt = now.AddHours(-2), HandledBy = manager, HandledAt = now.AddHours(-2) });

        // ---------- Incidents ----------
        db.Incidents.AddRange(
            new Incident { Camera = cam3, Type = DetectionType.Fall, Status = IncidentStatus.Active, Confidence = 0.91, CreatedAt = now },
            new Incident { Camera = cam5, Type = DetectionType.Fire, Status = IncidentStatus.Closed, Confidence = 0.88, Note = "Area checked and safe", Handler = ahmed, CreatedAt = now.AddHours(-1), ClosedAt = now.AddMinutes(-40) });

        await db.SaveChangesAsync();
    }

    private static Camera NewCamera(string name, string location, CameraHealth health, DateTime lastSeen, params DetectionType[] models) => new()
    {
        Name = name,
        Location = location,
        Health = health,
        LastSeenAt = lastSeen,
        StreamUrl = $"rtsp://demo/{name.Replace(" ", "").ToLower()}",
        ModelConfigs = models.Select(m => new CameraModelConfig { Type = m }).ToList()
    };
}
