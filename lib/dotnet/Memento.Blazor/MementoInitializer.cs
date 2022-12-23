using Memento.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Blazor;

public class MementoInitializer : ComponentBase {
    [Inject]
    public required StoreProvider StoreProvider { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await base.OnAfterRenderAsync(firstRender);
        await StoreProvider.InitializAsync();
    }
}