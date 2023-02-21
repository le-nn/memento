namespace Memento.Sample.Blazor.Stores;

public record WeatherForecast {
    public required DateTime Date { get; init; }

    public required int TemperatureC { get; init; }

    public required string? Summary { get; init; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}