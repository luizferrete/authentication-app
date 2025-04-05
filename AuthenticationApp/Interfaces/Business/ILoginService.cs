using AuthenticationApp.Domain.DTOs;

namespace AuthenticationApp.Interfaces.Business
{
    public interface ILoginService
    {
        public Task<UserDTO> Login(LoginDTO userDTO);
    }
}
