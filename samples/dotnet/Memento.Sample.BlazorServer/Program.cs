using Memento.Sample.Blazor;
using Memento.Sample.Blazor.Todos;
using Memento.Blazor;
using Memento.ReduxDevTool.Remote;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMemento()
    .AddMiddleware(() => new RemoteReduxDevToolMiddleware(8000))
    .ScanAssembyAndAddStores(typeof(App).Assembly);
builder.Services.AddSingleton<ITodoService, MockTodoService>();
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
