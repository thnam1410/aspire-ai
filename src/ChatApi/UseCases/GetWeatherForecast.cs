using FluentValidation;
using MediatR;
using Shared.Endpoint;

namespace ChatApi.UseCases;

public class GetWeatherForecast : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("weather-forecast", 
            async (ISender sender) => await sender.Send(new WeatherForecastQuery()));
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record WeatherForecastQuery : IRequest<IEnumerable<WeatherForecast>>;

internal class GetWeatherForecastQueryValidator : AbstractValidator<WeatherForecastQuery>{}

internal class GetWeatherForecastQueryHandler() : IRequestHandler<WeatherForecastQuery, IEnumerable<WeatherForecast>>
{
    public Task<IEnumerable<WeatherForecast>> Handle(WeatherForecastQuery request, CancellationToken cancellationToken)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        return Task.FromResult<IEnumerable<WeatherForecast>>(forecast);
    }
}