using PetroVision.BLL.DTOs;

namespace PetroVision.BLL.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<MessageResponseDto> SignupAsync(SignupRequestDto request);
    Task<UserDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task<LoginResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<MessageResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<MessageResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
}
