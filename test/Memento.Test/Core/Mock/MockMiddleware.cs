using Memento.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            IStateChangedEventArgs<object, Command> e,
            NextProviderMiddlewareCallback next
        ) {
            ProviderDispatchCalledCount++;
            return next(state, e);
        }

        public override object? HandleStoreDispatch(
            object? state,
            Command command,
            NextStoreMiddlewareCallback next
        ) {
            HandleStoreDispatchCalledCount++;
            return next(state, command);
        }
    }
}
