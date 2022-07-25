# Flux


### State

You should define state to manage in store before create store.
T with ```FluxStore<T>``` is the state type, plane object or class instance extended ```State<T>```.
State can be specified as plane object, but ```State<T>``` provides useful methods for immutable state.

The ```State<T>``` behavior is following. 

```ts
import { State } from "memento.core"

/**
 * State for counter.
 */
export class CounterState extends State<CounterState> {
    count = 0
    test = "foo"

    get next() {
        return this.count + 1
    }
}

const state = new CounterState()
console.log(state) // { count: 0, test: "foo" }

const state2 = state.clone({ count: 5 })
console.log(state2) // { count: 5,test: "foo" }

const state3 = state2.clone({
    count: state2.count * 3,
    test: "bar"
})
console.log(state3) // { count: 15, test: "bar" }
```

# Store

```FluxStore<T>```
