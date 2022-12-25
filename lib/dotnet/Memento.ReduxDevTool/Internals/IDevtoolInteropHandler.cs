using Memento.Core;
using System.Collections.Immutable;

namespace Memento.ReduxDevTool.Internal;

public interface IDevtoolInteropHandler {
    Action<string>? MessageHandled { get; set; }

    Task InitializeAsync(ImmutableDictionary<string, object> state);

    Task SendAsync(Command? command, HistoryStateContextJson context);

    void HandleMessage(string json);
}