import { meta, FluxStore, Message } from "memento.react";

const simpleState = {
    count: 0,
};

type SimpleCounterState = typeof simpleState

class Increment extends Message { }
class Decrement extends Message { }

@meta({ name: "FluxCounterStore" })
export class FluxCounterStore extends FluxStore<SimpleCounterState> {
    constructor() {
        super(simpleState, FluxCounterStore.mutation);
    }

    static mutation(state: SimpleCounterState, message: Message): SimpleCounterState {
        switch (message.comparer) {
            case Increment:
                return {
                    count: state.count + 1
                }
            case Decrement:
                return {
                    count: state.count - 1
                }
            default: throw new Error()
        }
    }

    async increment() {
        this.mutate(new Increment())
    }

    async decrement() {
        this.mutate(new Decrement())
    }
}
