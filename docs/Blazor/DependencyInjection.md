# Dependency Injection

Memento uses blazor default DI container.
DI is an abbreviation for Dependency Inject.
Services implemented as side effects such as 
HTTP Requests
asynchronous,
DB access,
and algorithm implementation 
can be accessed from actions using dependency injection.
It makes to be able to split any features.


## What's DI ?

dependency injection is a design pattern in which an object receives other objects
that it depends on. A form of inversion of control, 
dependency injection aims to separate the concerns of constructing objects and using them, 
leading to loosely coupled programs.

Dependency injection is basically providing the objects that an object needs (its dependencies) instead of having it construct them itself. It's a very useful technique for testing, since it allows dependencies to be mocked or stubbed out.

## Assignment decorator

After that, just specify the type in the constructor argument and it will be assigned automatically without doing anything special.
Blazor is supported as standard and does not need to be installed separately.

## Usage

You must register a Service to builder.

```cs
builder.Services.AddScoped(sp => new HttpClient {
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});
builder.Services.AddScoped<IAnyService, AnyService>();
```

## Example 1

Create a service class that generate fibonacci number.

```cs
using Memento;
using Memento.Blazor;

public record FibState {
    public int N { get; init; } = 0;
    public int Count { get; init; } = 0;
    public ImmutableArray<int> History { get; init; } = ImmutableArray.Create<int>();
}

public class FibonacciService {
    public int Fib(int n) {
        if (n < 3) {
            return 1;
        }
        
        return this.Fib(n - 1) + this.Fib(n - 2);
    }
}

public record FibMessages : Message {
    public record SetFib(int Value) : FibMessages;
}


public class FibStore : Store<FibState, FibMessages> {
    FibonacciService FibonacciService { get; }

    public FibStore(FibonacciService fibService) : base(() => new(), Mutation) {
        this.FibonacciService = fibService;
    }

    static FibState Mutation(FibState state, FibMessages message) {
        return message switch {
            FibMessages.SetFib payload => state with {
                N = state.N + 1,
                Count = payload.Value,
                History = state.History.Add(payload.Value),
            }
        };
    }

    public  void Calc() {
        if (this.State.N >= 40) {
            return;
        }

        var fib = this.FibonacciService.Fib(this.State.N);
        this.Mutate(new FibMessages.SetFib(fib));
    }
}
```

## Example 2 - Fetch data with HttpClient

Register HttpClient.
```cs
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

```

Define store.

```cs
using Memento;
using System.Collections.Immutable;
using System.Net.Http.Json;

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

```

Views 

```razor
@page "/fetchdata"
@using Blazor.Sample.Stores
@inject FetchDataStore FetchDataStore

<PageTitle>Weather forecast</PageTitle>

<h1>Weather forecast</h1>

<p>This component demonstrates fetching data from the server.</p>

@if (FetchDataStore.State.WeatherForecasts is null) {
    <p><em>Loading...</em></p>
}
else {
    <table class="table">
        <thead>
            <tr>
                <th>Date</th>
                <th>Temp. (C)</th>
                <th>Temp. (F)</th>
                <th>Summary</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var forecast in FetchDataStore.State.WeatherForecasts) {
                <tr>
                    <td>@forecast.Date.ToShortDateString()</td>
                    <td>@forecast.TemperatureC</td>
                    <td>@forecast.TemperatureF</td>
                    <td>@forecast.Summary</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    protected override async Task OnInitializedAsync() {
        await FetchDataStore.FetchAsync();
    }
}

```

# Next

[API Refelences](./API.md)
