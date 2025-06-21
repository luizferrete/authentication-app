using System.Net;
using System.Net.Http;
using System.Text.Json;
using AuthenticationApp.Domain.Exceptions;
using AuthenticationApp.Domain.Response;
using AuthenticationApp.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AuthenticationApp.Tests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task UnhandledException_ShouldReturnInternalServerError()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.Configure(app =>
                    {
                        app.UseMiddleware<ExceptionHandlingMiddleware>();
                        app.Run(_ => throw new Exception("boom"));
                    });
                })
                .StartAsync();

            var client = host.GetTestClient();
            var response = await client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var api = JsonSerializer.Deserialize<ApiResponse<object>>(content);
            Assert.False(api!.Success);
            Assert.Contains("boom", api.Errors!.First());
        }

        [Fact]
        public async Task ValidationException_ShouldReturnBadRequest()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.Configure(app =>
                    {
                        app.UseMiddleware<ExceptionHandlingMiddleware>();
                        app.Run(_ => throw new ValidationException(new[] { "field" }, "invalid"));
                    });
                })
                .StartAsync();

            var client = host.GetTestClient();
            var response = await client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var api = JsonSerializer.Deserialize<ApiResponse<object>>(content);
            Assert.False(api!.Success);
            Assert.Contains("field", api.Errors!.First());
        }
    }
}
