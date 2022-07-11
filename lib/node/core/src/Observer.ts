import { Message } from "./Message";
import { StateChangedEventArgs } from "./StateChangedEventArgs";

export type Observer<TState, TMessage extends Message> =
    (e: StateChangedEventArgs<TState, TMessage>) => void;
