using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using AuthenticationApp.Domain.Response;
using AuthenticationApp.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AppValidationException = AuthenticationApp.Domain.Exceptions.ValidationException;

namespace AuthenticationApp.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder routes)
        {
            var authRoutes = routes.MapGroup("/auth")
               .WithTags("Authentication")
               .WithOpenApi();

            authRoutes.MapPost("/login", async ([FromBody] LoginRequest login, IAuthService loginService, IValidator<LoginRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(login);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                    throw new AppValidationException(errors);
                }

                var response = await loginService.Login(login);

                return Results.Ok(ApiResponse<LoginResponse>.CreateSuccess(response, "Login realizado com sucesso"));

            })
            .WithName("LoginUser")
            .WithDescription("Logs in a user.");

            authRoutes.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService loginService, IValidator<RefreshTokenRequest> validator) =>
            {
                var validationRequest = await validator.ValidateAsync(request);

                if (!validationRequest.IsValid)
                {
                    var errors = validationRequest.Errors.Select(e => e.ErrorMessage);
                    throw new AppValidationException(errors);
                }

                var response = await loginService.RefreshToken(request);

                return Results.Ok(ApiResponse<LoginResponse>.CreateSuccess(response, "Token atualizado"));

            })
            .WithName("Refresh")
            .WithDescription("Refresh token login.");

            authRoutes.MapPost("/logout", async ([FromBody] RefreshTokenRequest request, IAuthService loginService) =>
            {
                var result = await loginService.Logout(request);

                return result
                    ? Results.Ok(ApiResponse<object>.CreateSuccess(null!, "Logout realizado"))
                    : Results.NotFound(ApiResponse<object>.CreateFailure("Recurso não encontrado"));

            })
           .RequireAuthorization()
           .WithName("Logout")
           .WithDescription("Logs out a user.");

            authRoutes.MapPost("/masslogout", async (IAuthService loginService) =>
            {
                var result = await loginService.MassLogout();

                return result
                    ? Results.Ok(ApiResponse<object>.CreateSuccess(null!, "Logout em massa realizado"))
                    : Results.NotFound(ApiResponse<object>.CreateFailure("Recurso não encontrado"));

            })
           .RequireAuthorization()
           .WithName("MassLogout")
           .WithDescription("Logs out the user from every device logged in.");

            return routes;
        }
    }
}
