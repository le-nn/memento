using Memento.Core;
using Microsoft.AspNetCore.Components;

namespace Memento.Blazor;

/// <summary>
/// The component for initializing Memento instance.
/// </summary>
public class MementoInitializer : ComponentBase, IDisposable {
    [Inject]
    public required StoreProvider StoreProvider { get; set; }

    [Parameter]
    public EventCallback Initialized { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender) {
            await InvokeAsync(async () => {
                await StoreProvider.InitializeAsync();
                await Initialized.InvokeAsync();
            });
        }
    }

    public void Dispose() {
        StoreProvider.Dispose();
    }
}