# Tutorial for typescript on node.js or browser

Let's take a look at an example of a simple console application to experience the Basic Concept.

# Install

Prease install via npm or yarn.

```
yarn add memento.js
```

or

```
npm install --save memento.js
```

### Typescript

Define store, state and messages.

```ts
import {
    meta,
    FluxStore,
    State,
    Message,
    createProvider
} from "memento.js"

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


// can be omitted this.
// if you want to omit, you should be following 
// "class FluxAsyncCounterStore extends FluxStore<FluxAsyncCounterState>"

/**
 * 
 */
@meta({ name: "FluxAsyncCounterStore" }) // Specify store name
export class FluxAsyncCounterStore extends FluxStore<FluxAsyncCounterState, FluxAsyncCounterMessages> {
    constructor() {
        super(new FluxAsyncCounterState(), FluxAsyncCounterStore.mutation)
    }

    // State can change via mutation and easy to observe state from message
    // Mutation generate new state from message and current state
    static mutation(
        state: FluxAsyncCounterState,
        message: FluxAsyncCounterMessages
        ) :FluxAsyncCounterState {
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
    async countUpAsync() {
        this.mutate(new BeginLoading())

        await delay(500)

        this.mutate(new Increment())
        this.mutate(new EndLoading())
    }

    setCount(num: number) {
        this.mutate(new ModifyCount({ count: num, }))
    }
}

```

### Usage

```ts
// Create stores and provider
export const provider = createProvider({
    stores: [
        FluxAsyncCounterStore,
    ]
})

// Observe all stores state
provider.subscribe(e => {
    console.log(e.present)
})

const store = provider.resolve(FluxAsyncCounterStore)
// Observe a store state
store.subscribe(e => {
    console.log(e.present)
})

// Call action and countup async.
await store.countUpAsync()
// Call action and set count.
store.setCount(5)
```

store.subscribe can be exptected output following

```json
// Initial
{
    "count": 0,
    "history": [],
    "isLoading": false
}

// countUpAsync
// BeginLoading
{
    "count": 0,
    "history": [],
    "isLoading": true
}
// 500ms later
// Increment
{
    "count": 1,
    "history": [
        1
    ],
    "isLoading": true
}
// EndLoading
{
    "count": 1,
    "history": [
        1
    ],
    "isLoading": false
}

// setCount
// ModifyCount
{
    "count": 5,
    "history": [
        1,
        5
    ],
    "isLoading": false
}
```
