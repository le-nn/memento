using Memento.Blazor;
using Memento.ReduxDevTool.Browser;
using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Todos;
using Memento.Samples.Blazor.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7288/") })
    .AddScoped<ITodoService, MockTodoService>()
    // Memento
    .AddMemento()
    .AddMiddleware(() => new ServerLoggerMiddleware())
    .AddBrowserReduxDevToolMiddleware(new() {
        StackTraceEnabled = true,
        OpenDevTool = true,
    }, true)
    .ScanAssemblyAndAddStores(typeof(Memento.Sample.Blazor._Imports).Assembly);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
}
else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<Memento.Samples.Blazor.WebApp.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Memento.Sample.Blazor.App).Assembly) // shared project
    .AddAdditionalAssemblies(typeof(Memento.Samples.Blazor.WebApp.Client._Imports).Assembly); // client project

app.Run();
