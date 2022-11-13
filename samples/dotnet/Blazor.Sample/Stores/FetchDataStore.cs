using Memento.Core;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace Blazor.Sample.Stores;

public record FetchDataState {
    public ImmutableArray<WeatherForecast>? WeatherForecasts { get; init; }
}

public record FetchDataCommands : Command {
    public record SetWeatherForecast(ImmutableArray<WeatherForecast> Items) : FetchDataCommands;
}

public record WeatherForecast {
    public required DateTime Date { get; init; }

    public required int TemperatureC { get; init; }

    public required string? Summary { get; init; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class FetchDataStore : Store<FetchDataState, FetchDataCommands> {
    HttpClient HttpClient { get; }

    public FetchDataStore(
        HttpClient httpClient
    ) : base(() => new(), Mutateion) {
        HttpClient = httpClient;
    }

    static FetchDataState Mutateion(FetchDataState state, FetchDataCommands command) {
        return command switch {
            FetchDataCommands.SetWeatherForecast payload => state with {
                WeatherForecasts = payload.Items,
            },
            _ => throw new Exception("The command is not handled."),
        };
    }

    public async Task FetchAsync() {
        var forecasts = await HttpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json")
            ?? throw new Exception("Failed to fetch data.");
        Mutate(new FetchDataCommands.SetWeatherForecast(forecasts.ToImmutableArray()));
    }
}
