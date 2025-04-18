using AuthenticationApp.Domain.Request;
using AuthenticationApp.Domain.Response;

namespace AuthenticationApp.Interfaces.Business
{
    public interface IAuthService
    {
        public Task<LoginResponse> Login(LoginRequest userDTO);
        public Task<LoginResponse> RefreshToken(RefreshTokenRequest refreshToken);

        public Task<bool> Logout();
    }
}
