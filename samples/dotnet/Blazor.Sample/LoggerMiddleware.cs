using Memento.Core;

namespace Blazor.Sample;

public class LoggerMiddleware : Middleware {
    public override object? Handle(object state, Command command, NextMiddlewareHandler next) {
        // Console.WriteLine(state);
        return next(state, command);
    }
}