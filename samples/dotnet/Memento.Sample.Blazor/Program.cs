using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Stores;
using Memento.Sample.Blazor.Todos;
using Memento.Blazor;
using Memento.Blazor.Devtools;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ITodoService, MockTodoService>();

builder.Services
    .AddMemento()
    .AddMiddleware(() => new LoggerMiddleware())
    .AddMiddleware(() => new BrowserReduxDevToolMiddleware())
    .ScanAssembyAndAddStores(typeof(Program).Assembly);

var app = builder.Build();
await app.RunAsync();