using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<MessageResponseDto> SignupAsync(SignupRequestDto request);
    Task<UserDto> GetProfileAsync(int userId);
}
