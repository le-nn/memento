import { Message } from "./Message";
import { FluxStore } from "./FluxStore";

class Mutate<TState> extends Message<TState>{ }

export abstract class LiteStore<TState = object> extends FluxStore<TState, Message> {
    constructor(initialState: TState) {
        super(initialState, LiteStore.mutation);
    }

    static mutation(_: any, message: Message): any {
        return message.payload;
    }

    public setState(state: TState) {
        super.mutate(new Mutate(state) as any);
    }
}
