using AuthenticationApp.Domain.Request;
using AuthenticationApp.Domain.Response;

namespace AuthenticationApp.Interfaces.Business
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest userDTO);
        Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshToken);
        Task<bool> Logout(RefreshTokenRequest request);
        Task<bool> MassLogout();
    }
}
