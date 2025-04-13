﻿using AuthenticationApp.Domain.DTOs;
using AuthenticationApp.Domain.Request;
using AuthenticationApp.Interfaces.Business;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace AuthenticationApp.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder routes)
        {
            var authRoutes = routes.MapGroup("/auth")
               .WithTags("Authentication")
               .WithOpenApi();

            authRoutes.MapPost("/login", async ([FromBody] LoginDTO login, IAuthService loginService) =>
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
            .WithDescription("Logs in a user.");

            authRoutes.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService loginService) =>
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
            .WithDescription("Refresh token login.");

            authRoutes.MapPost("/logout", async (IAuthService loginService) =>
            {
                try
                {
                    var result = await loginService.Logout();

                    return result ? Results.Ok() : Results.NotFound();
                }
                catch (Exception e)
                {
                    return Results.BadRequest(e);
                }

            })
           .RequireAuthorization()
           .WithName("Logout")
           .WithDescription("Logs out a user.");

            return routes;
        }
    }
}
