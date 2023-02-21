using Memento.Core;
using Microsoft.AspNetCore.Components;

namespace Memento.Blazor;

public class MementoInitializer : ComponentBase, IDisposable {
    [Inject]
    public required StoreProvider StoreProvider { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);
        await StoreProvider.InitializeAsync();
    }

    public void Dispose() {
        StoreProvider.Dispose();
    }
}