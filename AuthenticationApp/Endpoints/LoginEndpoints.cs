using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace AuthenticationApp.Endpoints
{
    public static class LoginEndpoints
    {
        public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/login", async ([FromBody] LoginDTO login, ILoginService loginService) =>
            {
                try
                {
                    var user = await loginService.Login(login);

                    return Results.Ok(user);
                }
                catch (InvalidCredentialException)
                {
                    return Results.Unauthorized();
                }

            })
            .WithName("LoginUser")
            .WithDescription("Logs in a user.")
            .WithOpenApi();

            return routes;
        }
    }
}
