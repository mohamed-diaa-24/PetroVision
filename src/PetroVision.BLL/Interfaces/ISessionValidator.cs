namespace PetroVision.BLL.Interfaces;

// Runs on every authenticated request (after the JWT signature is checked).
public interface ISessionValidator
{
    Task<bool> ValidateAsync(int userId, string? securityStamp);
}
