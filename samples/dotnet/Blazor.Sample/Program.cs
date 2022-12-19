using Blazor.Sample;
using Blazor.Sample.Stores;
using Blazor.Sample.Todos;
using Memento.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ITodoService, MockTodoService>();

builder.Services.AddMemento()
    .AddMiddleware<LoggerMiddleware>()
    .AddStore<AsyncCounterStore>()
    .AddStore<RedoUndoTodoStore>()
    .AddStore<FetchDataStore>();

var app = builder.Build();

app.UseStores();

await app.RunAsync();