using Memento.Core;
using Memento.ReduxDevTool.Internals;

namespace Memento.ReduxDevTool;

public interface IDevToolInteropHandler {
    Action<string>? MessageHandled { get; set; }

    Action? SyncRequested { get; set; }

    Task InitializeAsync(RootState state);

    Task SendAsync(Command? command, HistoryStateContextJson context);

    void HandleMessage(string json);
}