using Memento;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace Blazor.Sample.Stores;

public record FetchDataState {
    public ImmutableArray<WeatherForecast>? WeatherForecasts { get; init; }
}

public record FetchDataMessage : Message {
    public record SetWeatherForecast(ImmutableArray<WeatherForecast> Items) : FetchDataMessage;
}

public record WeatherForecast {
    public required DateTime Date { get; init; }

    public required int TemperatureC { get; init; }

    public required string? Summary { get; init; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class FetchDataStore : Store<FetchDataState, FetchDataMessage> {
    HttpClient HttpClient { get; }

    public FetchDataStore(
        HttpClient httpClient
    ) : base(() => new(), Mutateion) {
        this.HttpClient = httpClient;
    }

    static FetchDataState Mutateion(FetchDataState state, FetchDataMessage message) {
        return message switch {
            FetchDataMessage.SetWeatherForecast payload => state with {
                WeatherForecasts = payload.Items,
            },
            _ => throw new Exception("The message is not handled."),
        };
    }

    public async Task FetchAsync() {
        var forecasts = await this.HttpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json")
            ?? throw new Exception("Failed to fetch data.");
        this.Mutate(new FetchDataMessage.SetWeatherForecast(forecasts.ToImmutableArray()));
    }
}
