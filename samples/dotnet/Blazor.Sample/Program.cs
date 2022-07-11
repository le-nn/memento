using Blazor.Sample;
using Blazor.Sample.Stores;
using Memento.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMemento()
    .AddMiddleware<LoggerMiddleware>()
    .AddStore<AsyncCounterStore>()
    .AddStore<FetchDataStore>();

var app = builder.Build();
app.UseStores()
    .RunAsync();
