import { Message } from "./Message";

export type MutationMethod<TState, TMessage extends Message> =
    (state: TState, message: TMessage) => TState;