# memento.core

Flexible and easy unidirectional store-pattern container for state management with Dependency Injection for Frontend app on JS/TS.

You can define stores inspired by MVU patterns such as Flux and Elm to observe state changes more detail.

Some are inspired by Elm and MVU.
And Redux and Flux pattern are same too, but memento is not Redux and Flux.

Details and Docs on [Github](https://github.com/le-nn/memento)

#### Features

* Less boilarplate and simple usage 
* Is not flux or redux
* Observe detailed status with message patterns
* Immutable and Unidirectional data flow
* Multiple stores but manged by single provider, so can observe and manage as one state tree
* Less rules have been established
* Fragile because there are fewer established rules than Redux and Flux

# Code overview

Image of stores and mutate state.

```ts
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

```

### Usage

```ts
export const provider = createProvider({
    stores: [
        FluxAsyncCounterStore,
    ]
})

const store = provider.resolve(FluxAsyncCounterStore)

store.subscribe(e => {
    console.log(e.present)
})

console.log(store.state)

store.increment()
store.increment()
store.increment()
store.decrement()
store.increment()
store.increment()
store.decrement()
store.decrement()
store.increment()
store.increment()
```

store.subscribe can be exptected output following

```json
{ "count": 0 }
{ "count": 1 }
{ "count": 2 }
{ "count": 3 }
{ "count": 2 }
{ "count": 3 }
{ "count": 4 }
{ "count": 3 }
{ "count": 2 }
{ "count": 3 }
{ "count": 4 }
```

# License
Designed with ♥ le-nn. Licensed under the MIT License.
