using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

// Notes & Team: announcements and shift handover notes of the user's department.
public interface INotesService
{
    Task<List<AnnouncementDto>> GetAnnouncementsAsync();
    Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementDto dto);
    Task<AnnouncementDto> MarkAnnouncementSeenAsync(int id);
    Task<AnnouncementDto> SetPinnedAsync(int id, bool isPinned);
    Task DeleteAnnouncementAsync(int id);

    Task<List<HandoverNoteDto>> GetHandoversAsync(int take);
    Task<HandoverNoteDto> CreateHandoverAsync(CreateHandoverNoteDto dto);
    Task<HandoverNoteDto> MarkHandoverReadAsync(int id);
}
