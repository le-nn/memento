using Memento.Core;

namespace Memento.Test.Core.Mock;

public class MockMiddleware : Middleware {
    public new MockMiddlewareHandler? Handler { get; set; }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        Handler = new MockMiddlewareHandler();
        return Handler;
    }

    public class MockMiddlewareHandler : MiddlewareHandler {
        public int ProviderDispatchCalledCount { get; private set; }
        public int HandleStoreDispatchCalledCount { get; private set; }

        public override RootState? HandleProviderDispatch(
            RootState? state,
            IStateChangedEventArgs e,
            NextProviderMiddlewareCallback next
        ) {
            ProviderDispatchCalledCount++;
            return next(state, e);
        }

        public override object? HandleStoreDispatch(
            object? state,
            object? command,
            NextStoreMiddlewareCallback next
        ) {
            HandleStoreDispatchCalledCount++;
            return next(state, command);
        }
    }
}
