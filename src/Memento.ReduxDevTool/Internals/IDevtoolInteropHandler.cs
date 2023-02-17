using Memento.Core;
using Memento.Core.Store;

namespace Memento.ReduxDevTool.Internal;

public interface IDevtoolInteropHandler {
    Action<string>? MessageHandled { get; set; }

    Action? SyncRequested { get; set; }

    Task InitializeAsync(RootState state);

    Task SendAsync(Command? command, HistoryStateContextJson context);

    void HandleMessage(string json);
}