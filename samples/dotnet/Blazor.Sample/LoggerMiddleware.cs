using Memento;

namespace Blazor.Sample;

public class LoggerMiddleware : Middleware {
    public override object? Handle(object state, Message message, NextMiddlewareHandler next) {
        Console.WriteLine(state);
        return next(state, message);
    }
}
