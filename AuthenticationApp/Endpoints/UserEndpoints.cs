using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace AuthenticationApp.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/user", async ([FromBody] CreateUserDTO user, IUserService userService) =>
            {
                try
                {
                    await userService.CreateUser(user);
                    return Results.Created();
                } catch(Exception e)
                {
                    return Results.BadRequest(e.Message);
                }
            })
            .WithName("CreateUser")
            .WithDescription("Creates a new user.")
            .WithOpenApi();

            routes.MapPost("/user/changepassword", async ([FromBody] ChangePasswordRequest changePassword, IUserService userService) =>
            {
                try
                {
                    await userService.ChangePassword(changePassword);
                    return Results.Ok();
                }
                catch (InvalidOperationException e)
                {
                    return Results.BadRequest(e.Message);
                }
                catch (InvalidCredentialException)
                {
                    return Results.Unauthorized();
                }
            })
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithDescription("Changes the password of a user.")
            .WithOpenApi();

            return routes;
        }
    }
}
