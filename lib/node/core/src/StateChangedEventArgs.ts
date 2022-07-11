import { Message } from "./Message";

export interface StateChangedEventArgs<TState = object, TMessage extends Message = Message> {
    present: TState;
    previous: TState;
    store: string;
    message: TMessage;
    timestamp: number;
    stacktrace?: string;
}