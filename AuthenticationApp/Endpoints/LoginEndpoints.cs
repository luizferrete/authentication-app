using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
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
                    var response = await loginService.Login(login);

                    return Results.Ok(response);
                }
                catch (InvalidCredentialException)
                {
                    return Results.Unauthorized();
                }

            })
            .WithName("LoginUser")
            .WithDescription("Logs in a user.")
            .WithOpenApi();

            routes.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, ILoginService loginService) =>
            {
                try
                {
                    var response = await loginService.RefreshToken(request);

                    return Results.Ok(response);
                }
                catch (InvalidCredentialException)
                {
                    return Results.Unauthorized();
                }

            })
            .WithName("Refresh")
            .WithDescription("Refresh token login.")
            .WithOpenApi();

            routes.MapPost("/logout", async (ILoginService loginService) =>
            {
                try
                {
                    await loginService.Logout();

                    return Results.Ok();
                }
                catch (Exception e)
                {
                    return Results.BadRequest(e);
                }

            })
           .RequireAuthorization()
           .WithName("Logout")
           .WithDescription("Logs out a user.")
           .WithOpenApi();

            return routes;
        }
    }
}
