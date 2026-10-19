using PetroVision.DAL.Entities;

namespace PetroVision.BLL.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
