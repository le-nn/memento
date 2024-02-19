using Memento.Blazor;
using Memento.ReduxDevTool.Browser;
using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Todos;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
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
    .ScanAssemblyAndAddStores(typeof(App).Assembly);

var app = builder.Build();
await app.RunAsync();