import { constructor } from "./constructor";
import { Message } from "./Message";
import { Observer } from "./Observer";
import { FluxStore } from "./FluxStore";
import { StateChangedEventArgs } from "./StateChangedEventArgs";
import { Subscription } from "./Subscription";

class Mutate<TState> extends Message<TState>{ }

export abstract class LiteStore<TState = object> extends FluxStore<TState, Mutate<TState>> {
    constructor(initialState: TState) {
        super(initialState, LiteStore.mutation);
    }

    static mutation(_: any, message: Message): any {
        return message.payload;
    }

    public setState(state: TState) {
        super.mutate(new Mutate(state));
    }
}
