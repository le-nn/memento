import { meta, LiteStore } from "memento.react";

const delay = (timeout: number) =>
    new Promise(resolve => setTimeout(resolve, timeout))

const simpleState = {
    count: 0,
    history: [] as number[],
    isLoading: false,
};

@meta({ name: "LiteAsyncCounterStore" })
export class LiteAsyncCounterStore extends LiteStore<typeof simpleState> {
    constructor() {
        super(simpleState);
    }

    async countUpAsync() {
        this.setState({
            ...this.state,
            isLoading: true,
        })

        await delay(500);

        const state = this.state;
        const count = state.count + 1

        this.setState({
            ...state,
            history: [...state.history, count],
            count,
            isLoading: false,
        })
    }

    setCount(num: number) {
        this.setState({
            ...this.state, count: num,
            history: [...this.state.history, num]
        })
    }
}
