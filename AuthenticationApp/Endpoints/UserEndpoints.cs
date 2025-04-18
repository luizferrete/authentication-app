using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace AuthenticationApp.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
        {
            var userRoutes = routes.MapGroup("/user")
                .WithTags("Users")
                .WithOpenApi();

            userRoutes.MapPost("/", async ([FromBody] CreateUserRequest user, IUserService userService, IValidator<CreateUserRequest> validator) =>
            {
                try
                {
                    var validationResult = await validator.ValidateAsync(user);

                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

                    await userService.CreateUser(user);
                    return Results.Created();
                } 
                catch(Exception e)
                {
                    return Results.BadRequest(e.Message);
                }
            })
            .WithName("CreateUser")
            .WithDescription("Creates a new user.");

            userRoutes.MapPost("/changepassword", async ([FromBody] ChangePasswordRequest changePassword, IUserService userService, IValidator<ChangePasswordRequest> validator) =>
            {
                try
                {
                    var validationResult = await validator.ValidateAsync(changePassword);

                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary());
                    }

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
            .WithDescription("Changes the password of a user.");

            return routes;
        }
    }
}
