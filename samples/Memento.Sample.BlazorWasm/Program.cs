using Memento.Blazor;
using Memento.Blazor.Devtools.Browser;
using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Todos;
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
    .ScanAssembyAndAddStores(typeof(App).Assembly);

var app = builder.Build();
await app.RunAsync();