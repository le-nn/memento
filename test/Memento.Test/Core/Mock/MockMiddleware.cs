using Memento.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.Test.Core.Mock;

public class MockMiddleware : Middleware {
    public new MockMiddlewarehandler? Handler { get; set; }

    protected override MiddlewareHandler Create(IServiceProvider provider) {
        Handler = new MockMiddlewarehandler();
        return Handler;
    }

    public class MockMiddlewarehandler : MiddlewareHandler {
        public int ProviderDispatchCalledCount { get; private set; }
        public int HandleStoreDispatchCalledCount { get; private set; }

        public override RootState? HandleProviderDispatch(
            RootState state,
            StateChangedEventArgs e,
            NextProviderMiddlewareCallback next
        ) {
            ProviderDispatchCalledCount++;
            return next(state, e);
        }

        public override object? HandleStoreDispatch(
            object state,
            Command command,
            NextStoreMiddlewareCallback next
        ) {
            HandleStoreDispatchCalledCount++;
            return next(state, command);
        }
    }
}
