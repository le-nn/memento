using System.Collections.Immutable;
using System.Net.Http.Json;

namespace Memento.Sample.Blazor.Stores;

public record FetchDataState {
    public ImmutableArray<WeatherForecast>? WeatherForecasts { get; init; }
}

public class FetchDataStore(HttpClient httpClient) : Store<FetchDataState>(() => new()) {
    readonly HttpClient _httpClient = httpClient;

    public async Task FetchAsync() {
        var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json")
            ?? throw new Exception("Failed to fetch data.");
        Mutate(state => state with {
            WeatherForecasts = [.. forecasts],
        });
    }
}