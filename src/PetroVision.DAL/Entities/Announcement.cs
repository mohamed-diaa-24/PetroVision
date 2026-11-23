namespace PetroVision.DAL.Entities;

// Department-wide message from the manager (or an agent with Post Announcements).
public class Announcement
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department Department { get; set; } = null!;
    public User Author { get; set; } = null!;
    public ICollection<AnnouncementRead> Reads { get; set; } = new List<AnnouncementRead>();
}

// One row per user who opened it.
public class AnnouncementRead
{
    public int Id { get; set; }
    public int AnnouncementId { get; set; }
    public int UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public Announcement Announcement { get; set; } = null!;
    public User User { get; set; } = null!;
}
