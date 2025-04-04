using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationApp.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("/user", async ([FromBody] CreateUserDTO user, IUserService userService) =>
            {
                await userService.CreateUser(user);
            })
            .WithName("CreateUser")
            .WithDescription("Creates a new user.")
            .WithOpenApi();

            routes.MapPost("/user/login", async ([FromBody] LoginDTO login, IUserService userService) =>
            {
                var user = await userService.GetUserByCredentials(login.Username, login.Password);
                if (user == null)
                {
                    return Results.NotFound("User not found");
                }
                return Results.Ok(user);
            })
            .WithName("LoginUser")
            .WithDescription("Logs in a user.")
            .WithOpenApi();

            return routes;
        }
    }
}
