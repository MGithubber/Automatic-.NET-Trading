namespace Presentation.Api;

public static class Extensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // // to do // //
        
    }

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/weatherforecast", () => { })
        .WithName("GetWeatherForecast")
        .WithOpenApi();
    }
}
