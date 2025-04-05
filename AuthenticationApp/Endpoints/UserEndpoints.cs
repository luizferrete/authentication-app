using AuthenticationApp.Domain.DTOs;
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
                await userService.CreateUser(user);
            })
            .WithName("CreateUser")
            .WithDescription("Creates a new user.")
            .WithOpenApi();

            return routes;
        }
    }
}
