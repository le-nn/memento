import { meta, LiteStore, Message } from "memento.react";

const simpleState = {
    count: 0,
};

type SimpleCounterState = typeof simpleState

@meta({ name: "LiteCounterStore" })
export class LiteCounterStore extends LiteStore<SimpleCounterState> {
    constructor() {
        super(simpleState);
    }

    async increment() {
        this.setState({
            ...this.state,
            count: this.state.count + 1,
        })
    }

    async decrement() {
        this.setState({
            ...this.state,
            count: this.state.count - 1,
        })
    }
}
