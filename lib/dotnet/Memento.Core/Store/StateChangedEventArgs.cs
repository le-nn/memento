namespace Memento;

public abstract record StateChangedEventArgs {
    protected object? sender;

    public Message? Message { get; init; }
    public object? LastState { get; init; }
    public object? State { get; init; }
    public Store<object, Message>? Sender {
        get {
            return (Store<object, Message>)this.sender;
        }
        init {
            this.sender = value;
        }
    }
}

public record StateChangedEventArgs<TState, TMessage> : StateChangedEventArgs
    where TState : class
    where TMessage : Message {
    public new required TMessage Message {
        get => (TMessage)base.Message!;
        init => base.Message = value;
    }


    public new required TState LastState {
        get => (TState)base.LastState!;
        init => base.LastState = value;
    }

    public new required TState State {
        get => (TState)base.State!;
        init => base.State = value;
    }

    public new required Store<TState, TMessage> Sender {
        get => (Store<TState, TMessage>)base.sender!;
        init => base.sender = value;
    }
}
