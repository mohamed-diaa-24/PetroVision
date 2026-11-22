using System.ComponentModel.DataAnnotations;

namespace PetroVision.BLL.DTOs;

public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public int AuthorId { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SeenCount { get; set; }
    public int TeamSize { get; set; }              // "5 of 8 seen"
    public bool SeenByMe { get; set; }
}

public class CreateAnnouncementDto
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsPinned { get; set; }
}

public class PinAnnouncementDto
{
    public bool IsPinned { get; set; }
}

public class HandoverNoteDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool ReadByMe { get; set; }
    public List<string> ReadBy { get; set; } = new();
}

public class CreateHandoverNoteDto
{
    [Required, StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Details { get; set; } = string.Empty;
}
