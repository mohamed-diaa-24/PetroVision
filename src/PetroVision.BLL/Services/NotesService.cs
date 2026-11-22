using Microsoft.EntityFrameworkCore;
using PetroVision.BLL.DTOs;
using PetroVision.BLL.Exceptions;
using PetroVision.BLL.Interfaces;
using PetroVision.DAL.Context;
using PetroVision.DAL.Entities;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Services;

public class NotesService : INotesService
{
    private const int MaxHandovers = 200;

    private readonly PetroVisionDbContext _db;
    private readonly ICurrentUser _user;
    private readonly IAuditLogger _audit;

    public NotesService(PetroVisionDbContext db, ICurrentUser user, IAuditLogger audit)
    {
        _db = db;
        _user = user;
        _audit = audit;
    }

    private int DepartmentId => _user.DepartmentId
        ?? throw new ForbiddenException("Your account is not assigned to a department yet.");

    // ---------- Announcements ----------
    public async Task<List<AnnouncementDto>> GetAnnouncementsAsync()
    {
        var departmentId = DepartmentId;
        var teamSize = await TeamSizeAsync(departmentId);

        var items = await AnnouncementQuery(departmentId)
            .OrderByDescending(a => a.IsPinned).ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        items.ForEach(i => i.TeamSize = teamSize);
        return items;
    }

    public async Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementDto dto)
    {
        var announcement = new Announcement
        {
            DepartmentId = DepartmentId,
            Title = dto.Title.Trim(),
            Message = dto.Message.Trim(),
            IsPinned = dto.IsPinned,
            AuthorId = _user.UserId
        };
        // The author has obviously seen it.
        announcement.Reads.Add(new AnnouncementRead { UserId = _user.UserId });

        _db.Announcements.Add(announcement);
        await _db.SaveChangesAsync();

        _audit.Add(AuditAction.AnnouncementPosted, nameof(Announcement), announcement.Id, announcement.Title,
            announcement.IsPinned ? "Pinned" : null);
        await _db.SaveChangesAsync();

        return await GetAnnouncementAsync(announcement.Id);
    }

    public async Task<AnnouncementDto> MarkAnnouncementSeenAsync(int id)
    {
        var announcement = await LoadAnnouncementAsync(id);

        if (!await _db.AnnouncementReads.AnyAsync(r => r.AnnouncementId == id && r.UserId == _user.UserId))
        {
            _db.AnnouncementReads.Add(new AnnouncementRead { AnnouncementId = announcement.Id, UserId = _user.UserId });
            await _db.SaveChangesAsync();
        }

        return await GetAnnouncementAsync(id);
    }

    public async Task<AnnouncementDto> SetPinnedAsync(int id, bool isPinned)
    {
        var announcement = await LoadAnnouncementAsync(id);
        announcement.IsPinned = isPinned;
        await _db.SaveChangesAsync();

        return await GetAnnouncementAsync(id);
    }

    public async Task DeleteAnnouncementAsync(int id)
    {
        var announcement = await LoadAnnouncementAsync(id);

        // Agents with the permission may delete only what they posted; managers anything.
        if (_user.Role != "Manager" && announcement.AuthorId != _user.UserId)
            throw new ForbiddenException("You can delete only your own announcements.");

        _db.Announcements.Remove(announcement);
        _audit.Add(AuditAction.AnnouncementDeleted, nameof(Announcement), announcement.Id, announcement.Title);
        await _db.SaveChangesAsync();
    }

    private async Task<Announcement> LoadAnnouncementAsync(int id)
    {
        var departmentId = DepartmentId;
        return await _db.Announcements.FirstOrDefaultAsync(a => a.Id == id && a.DepartmentId == departmentId)
               ?? throw new NotFoundException("Announcement not found.");
    }

    private async Task<AnnouncementDto> GetAnnouncementAsync(int id)
    {
        var departmentId = DepartmentId;
        var item = await AnnouncementQuery(departmentId).FirstOrDefaultAsync(a => a.Id == id)
                   ?? throw new NotFoundException("Announcement not found.");

        item.TeamSize = await TeamSizeAsync(departmentId);
        return item;
    }

    private IQueryable<AnnouncementDto> AnnouncementQuery(int departmentId)
    {
        var me = _user.UserId;
        return _db.Announcements.AsNoTracking()
            .Where(a => a.DepartmentId == departmentId)
            .Select(a => new AnnouncementDto
            {
                Id = a.Id,
                Title = a.Title,
                Message = a.Message,
                IsPinned = a.IsPinned,
                AuthorId = a.AuthorId,
                Author = a.Author.FullName,
                CreatedAt = a.CreatedAt,
                SeenCount = a.Reads.Count,
                SeenByMe = a.Reads.Any(r => r.UserId == me)
            });
    }

    private Task<int> TeamSizeAsync(int departmentId)
        => _db.Users.CountAsync(u => u.DepartmentId == departmentId && u.Status == UserStatus.Active
                                     && (u.Role == UserRole.Manager || u.Role == UserRole.Agent));

    // ---------- Handover notes ----------
    public async Task<List<HandoverNoteDto>> GetHandoversAsync(int take)
    {
        var departmentId = DepartmentId;
        take = Math.Clamp(take, 1, MaxHandovers);

        return await HandoverQuery(departmentId).Take(take).ToListAsync();
    }

    public async Task<HandoverNoteDto> CreateHandoverAsync(CreateHandoverNoteDto dto)
    {
        var note = new HandoverNote
        {
            DepartmentId = DepartmentId,
            Title = dto.Title.Trim(),
            Details = dto.Details.Trim(),
            AuthorId = _user.UserId
        };

        _db.HandoverNotes.Add(note);
        await _db.SaveChangesAsync();

        _audit.Add(AuditAction.HandoverCreated, nameof(HandoverNote), note.Id, note.Title);
        await _db.SaveChangesAsync();

        return await GetHandoverAsync(note.Id);
    }

    public async Task<HandoverNoteDto> MarkHandoverReadAsync(int id)
    {
        var departmentId = DepartmentId;
        if (!await _db.HandoverNotes.AnyAsync(n => n.Id == id && n.DepartmentId == departmentId))
            throw new NotFoundException("Handover note not found.");

        if (!await _db.HandoverReads.AnyAsync(r => r.HandoverNoteId == id && r.UserId == _user.UserId))
        {
            _db.HandoverReads.Add(new HandoverRead { HandoverNoteId = id, UserId = _user.UserId });
            await _db.SaveChangesAsync();
        }

        return await GetHandoverAsync(id);
    }

    private async Task<HandoverNoteDto> GetHandoverAsync(int id)
        => await HandoverQuery(DepartmentId).FirstOrDefaultAsync(n => n.Id == id)
           ?? throw new NotFoundException("Handover note not found.");

    private IQueryable<HandoverNoteDto> HandoverQuery(int departmentId)
    {
        var me = _user.UserId;
        return _db.HandoverNotes.AsNoTracking()
            .Where(n => n.DepartmentId == departmentId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new HandoverNoteDto
            {
                Id = n.Id,
                Title = n.Title,
                Details = n.Details,
                AuthorId = n.AuthorId,
                Author = n.Author.FullName,
                CreatedAt = n.CreatedAt,
                ReadByMe = n.AuthorId == me || n.Reads.Any(r => r.UserId == me),
                ReadBy = n.Reads.OrderBy(r => r.ReadAt).Select(r => r.User.FullName).ToList()
            });
    }
}
