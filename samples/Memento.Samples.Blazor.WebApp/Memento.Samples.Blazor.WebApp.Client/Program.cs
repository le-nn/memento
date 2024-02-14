using Memento.Blazor;
using Memento.ReduxDevTool.Browser;
using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Todos;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddScoped<ITodoService, MockTodoService>()
    // Memento
    .AddMemento()
    .AddMiddleware(() => new LoggerMiddleware())
    .AddBrowserReduxDevToolMiddleware(new() {
        StackTraceEnabled = true,
        OpenDevTool = true,
    })
    .ScanAssemblyAndAddStores(typeof(Memento.Sample.Blazor.App).Assembly);
await builder.Build().RunAsync();
