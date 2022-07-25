import {
    meta,
    FluxStore,
    State,
    Message,
    createProvider,
} from "memento.core"

const delay = (timeout: number) =>
    new Promise(resolve => setTimeout(resolve, timeout))

// Define state to manage in store
class FluxAsyncCounterState extends State<FluxAsyncCounterState> {
    count = 0
    history: number[] = []
    isLoading = false
}

// Define messages to mutate state and observe state change event in detail.
class Increment extends Message { }
class BeginLoading extends Message { }
class EndLoading extends Message { }
class ModifyCount extends Message<{ count: number }> { }

type FluxAsyncCounterMessages =
    Increment
    | BeginLoading
    | EndLoading
    | ModifyCount

@meta({ name: "FluxAsyncCounterStore" })
export class FluxAsyncCounterStore
    extends FluxStore<FluxAsyncCounterState, FluxAsyncCounterMessages> {
    constructor() {
        super(new FluxAsyncCounterState(), FluxAsyncCounterStore.mutation)
    }

    // State can change via mutation and easy to observe state from message
    // Mutation generate new state from message and current state
    static mutation(
        state: FluxAsyncCounterState,
        message: FluxAsyncCounterMessages
    ): FluxAsyncCounterState {
        switch (message.comparer) {
            case BeginLoading: {
                return state.clone({
                    isLoading: true
                })
            }
            case EndLoading: {
                return state.clone({
                    isLoading: false
                })
            }
            case Increment: {
                const count = state.count + 1
                return state.clone({
                    count,
                    history: [...state.history, count]
                })
            }
            case ModifyCount: {
                const { payload } = message as ModifyCount
                return state.clone({
                    count: payload.count,
                    history: [...state.history, payload.count]
                })
            }
            default: throw new Error("Message is not handled")
        }
    }

    // "mutate" method can called outside of store via action (pub lic method)
    // Action can be async method.
    public async countUpAsync() {
        this.mutate(new BeginLoading())

        await delay(500)

        this.mutate(new Increment())
        this.mutate(new EndLoading())
    }

    public setCount(num: number) {
        this.mutate(new ModifyCount({ count: num, }))
    }
}
