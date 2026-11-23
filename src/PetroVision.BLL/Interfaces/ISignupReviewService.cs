using PetroVision.BLL.DTOs;
using PetroVision.DAL.Enums;

namespace PetroVision.BLL.Interfaces;

// Manager: agent sign-ups of their department. Owner: manager sign-ups.
public interface ISignupReviewService
{
    Task<List<SignupRequestItemDto>> GetPendingAsync();
    Task<SignupRequestItemDto> ApproveAsync(int userId, List<PermissionType> permissions);
    Task<SignupRequestItemDto> RejectAsync(int userId, string reason);
}
