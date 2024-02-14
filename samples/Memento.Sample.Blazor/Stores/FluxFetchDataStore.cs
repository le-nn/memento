using System.Collections.Immutable;
using System.Net.Http.Json;

namespace Memento.Sample.Blazor.Stores;

/// <summary>
/// Represents the state of the FluxFetchDataStore.
/// </summary>
public record FluxFetchDataState {
    /// <summary>
    /// Gets or initializes the weather forecasts.
    /// </summary>
    public ImmutableArray<WeatherForecast>? WeatherForecasts { get; init; }
}

/// <summary>
/// Represents the commands of the FluxFetchDataStore.
/// </summary>
public record FluxFetchDataCommands : Command {
    /// <summary>
    /// Represents the command to set the weather forecast.
    /// </summary>
    public record SetWeatherForecast(ImmutableArray<WeatherForecast> Items) : FluxFetchDataCommands;
}

/// <summary>
/// Represents the FluxFetchDataStore.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FluxFetchDataStore"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client.</param>
public class FluxFetchDataStore(
    HttpClient httpClient
    ) : FluxStore<FetchDataState, FluxFetchDataCommands>(() => new(), Reducer) {
    readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// The reducer function for the FluxFetchDataStore.
    /// </summary>
    /// <param name="state">The current state.</param>
    /// <param name="command">The command to be executed.</param>
    /// <returns>The updated state.</returns>
    static FetchDataState Reducer(FetchDataState state, FluxFetchDataCommands? command) {
        return command switch {
            FluxFetchDataCommands.SetWeatherForecast payload => state with {
                WeatherForecasts = payload.Items,
            },
            _ => throw new Exception("The command is not handled."),
        };
    }

    /// <summary>
    /// Fetches the weather data asynchronously.
    /// </summary>
    public async Task FetchAsync() {
        var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json")
            ?? throw new Exception("Failed to fetch data.");
        Dispatch(new FluxFetchDataCommands.SetWeatherForecast(forecasts.ToImmutableArray()));
    }
}
