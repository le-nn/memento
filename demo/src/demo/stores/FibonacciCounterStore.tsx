import { FluxStore, service, Message, meta, State, createProvider } from "memento.core"

type History = {
    value: number,
    n: number,
}

class FibState extends State<FibState> {
    n = 0
    count = 0
    history: History[] = []
}

@service()
export class FibonacciService {
    public fib(n: number): number {
        if (n < 3) return 1
        return this.fib(n - 1) + this.fib(n - 2)
    }
}

class SetFib extends Message<number> { }

@meta({ name: "FibonacciCounterStore" })
export class FibonacciCounterStore extends FluxStore<FibState> {
    constructor(readonly fibService: FibonacciService) {
        super(new FibState(), FibonacciCounterStore.mutation)
    }

    static mutation(state: FibState, message: Message): FibState {
        switch (message.comparer) {
            case SetFib:
                const { payload } = message as SetFib
                const n = state.n + 1
                return state.clone({
                    n,
                    count: payload,
                    history: [
                        ...state.history,
                        {
                            n,
                            value: payload,
                        }
                    ]
                })
            default: throw new Error("State is not handled")
        }
    }

    calc() {
        if (this.state.n < 40 === false) {
            return
        }

        const fib = this.fibService.fib(this.state.n)
        this.mutate(new SetFib(fib))
    }
}
