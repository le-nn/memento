using Memento.Blazor;
using Memento.ReduxDevTool.Remote;
using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Todos;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRemoteReduxDevToolMiddleware()
    .AddMemento()
    .AddMiddleware(() => new LoggerMiddleware())
    .ScanAssemblyAndAddStores(typeof(App).Assembly);

builder.Services.AddScoped<ITodoService, MockTodoService>();
builder.Services.AddSingleton(p => new HttpClient {
    BaseAddress = new Uri("https://localhost:7236")
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
