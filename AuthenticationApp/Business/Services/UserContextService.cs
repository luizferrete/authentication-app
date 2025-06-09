using AuthenticationApp.Interfaces.Business;
using System.Security.Claims;

namespace AuthenticationApp.Business.Services
{
    public class UserContextService(IHttpContextAccessor context) : IUserContextService
    {
        public string UserIpAddress => context.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        public string? UserName => context.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
    }
}
