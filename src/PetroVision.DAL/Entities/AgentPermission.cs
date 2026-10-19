using PetroVision.DAL.Enums;

namespace PetroVision.DAL.Entities;

public class AgentPermission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public PermissionType Permission { get; set; }

    public User User { get; set; } = null!;
}
