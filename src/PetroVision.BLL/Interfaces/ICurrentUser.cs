namespace PetroVision.BLL.Interfaces;

public interface ICurrentUser
{
    int UserId { get; }
    string Role { get; }
    int? DepartmentId { get; }
}
