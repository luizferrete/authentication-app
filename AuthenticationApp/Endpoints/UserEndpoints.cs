using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Domain.Response;
using AuthenticationApp.Domain.Exceptions;
using AppValidationException = AuthenticationApp.Domain.Exceptions.ValidationException;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                var validationResult = await validator.ValidateAsync(user);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    throw new AppValidationException(errors);
                }

                await userService.CreateUser(user);
                return Results.Created("/user", ApiResponse<object>.CreateSuccess(null!, "Usuário criado com sucesso"));
            })
            .WithName("CreateUser")
            .WithDescription("Creates a new user.");

            userRoutes.MapPost("/changepassword", async ([FromBody] ChangePasswordRequest changePassword, IUserService userService, IValidator<ChangePasswordRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(changePassword);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    throw new AppValidationException(errors);
                }

                await userService.ChangePassword(changePassword);
                return Results.Ok(ApiResponse<object>.CreateSuccess(null!, "Senha alterada com sucesso"));
            })
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithDescription("Changes the password of a user.");

            return routes;
        }
    }
}
