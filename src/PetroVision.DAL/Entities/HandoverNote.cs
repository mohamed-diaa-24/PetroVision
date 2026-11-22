namespace PetroVision.DAL.Entities;

// Shift handover: what the next shift must know.
public class HandoverNote
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department Department { get; set; } = null!;
    public User Author { get; set; } = null!;
    public ICollection<HandoverRead> Reads { get; set; } = new List<HandoverRead>();
}

// One row per user who opened it.
public class HandoverRead
{
    public int Id { get; set; }
    public int HandoverNoteId { get; set; }
    public int UserId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public HandoverNote HandoverNote { get; set; } = null!;
    public User User { get; set; } = null!;
}
