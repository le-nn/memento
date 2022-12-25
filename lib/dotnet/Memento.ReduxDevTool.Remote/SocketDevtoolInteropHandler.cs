using Memento.Core;
using Memento.ReduxDevTool.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.ReduxDevTool.Remote;

internal class SocketDevtoolInteropHandler : IDevtoolInteropHandler {
    public Action<string>? MessageHandled { get; set; }

    public void HandleMessage(string json) {

    }

    public Task InitializeAsync(ImmutableDictionary<string, object> state) {
        return Task.CompletedTask;
    }

    public Task SendAsync(Command? command, HistoryStateContextJson context) {
        return Task.CompletedTask;
    }
}
