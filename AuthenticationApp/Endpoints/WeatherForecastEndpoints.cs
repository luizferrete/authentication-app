using AuthenticationApp.Domain.Response;

namespace AuthenticationApp.Endpoints
{
    public static class WeatherForecastEndpoints
    {
        public static IEndpointRouteBuilder MapWeatherForecastEndpoints(this IEndpointRouteBuilder routes)
        {
            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            routes.MapGet("/weatherforecast", () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                    .ToArray();
                return Results.Ok(ApiResponse<WeatherForecast[]>.CreateSuccess(forecast));
            })
            .WithName("GetWeatherForecast")
            .RequireAuthorization()
            .WithOpenApi();

            return routes;
        }

        internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
        {
            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        }
    }
}
