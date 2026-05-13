using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDTO> RegisterAsync(RegisterDTO request);
    Task<AuthResponseDTO> LoginAsync(LoginDTO request);
}
