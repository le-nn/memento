using Memento.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Blazor.Devtools;

public interface IDevtoolInteropHandler {
    Task InitializeAsync(ImmutableDictionary<string, object> state);

    Task SendAsync(Command? command, HistoryStateContextJson context);

    void HandleMessage(string json);
}
