# Simpla Store

The most simple usecase is as a store container.
This is not supported observe detailed with message pattern.
Looks very simple and easy to handle,
but as it larger and becomes more complex, it can break the order.

# Store Overview

Let's take a look at the completed store first.
Dig into step by step from there.

```ts
import { LiteStore } from "memento.react"

const delay = (timeout: number) =>
    new Promise(resolve => setTimeout(resolve, timeout))

const simpleState = {
    count: 0,
    history: [],
    isLoading: false,
}

@meta({ name: "SimpleStore" })
export class SimpleStore extends LiteStore<typeof simpleState> {
    constructor() {
        super(simpleState)
    }

    async countUpAsync() {
        this.setState({
            ...this.state,
            isLoading: true,
        })

        await delay(500)

        const state = this.state
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

    async delay(timeout: number) {
        await new Promise(resolve => setTimeout(resolve, timeout))
    }
}

```

## State

You should define state to manage in store before create store.
T with ```LiteStore<T>``` is the state type, plane object or class instance extended ```State<T>```.
State can be specified as plane object, but ```State<T>``` provides useful methods for immutable state.

### Plane Object

```ts
const simpleState = {
    count: 0,
    history: [],
    isLoading: false,
}

```

### Extendsd state 

```ts
import { State } from "memento.js"

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

```

The ```State<T>``` behavior is following. 

```ts

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

## Define Store

Create class and extends ```LiteStore``` and specify initialState to super on constructor.
Specify initial state into args of super constructor

```ts
export class SimpleStore extends LiteStore<typeof simpleState> {
    constructor() {
        super(simpleState)
    }
}

```

or 

```ts
export class SimpleStore extends LiteStore<CounterState> {
    constructor() {
        super(new CounterState())
    }
}
```

### Store Name 

Specify store name with ```@meta``` decorator following

```ts
@meta({ name: "SimpleStore" })
export class SimpleStore extends LiteStore<...> {

}

```

If js or you do not want to use decorator.
You should define static member following.

```ts
export class SimpleStore extends LiteStore<...> {
    static storeName = "SimpleStore"
}

```

### State changes

To change the state, define a method as an action in the store.
Change the state by calling it from the outside.

* Call ```this.setState(state:T)``` to update store state

The following example defines ```countUpAsync```.

#### State change steps
* Loading state to true
* Run async timer
* Set the final state 

```ts
async countUpAsync() {
    this.setState({
        ...this.state,
        isLoading: true,
    })

    await delay(500)

    const state = this.state
    const count = state.count + 1

    this.setState({
        ...state,
        history: [...state.history, count],
        count,
        isLoading: false,
    })
}

```

Set state directory in ```setCount```.
```update``` is public method, so you can call from outside of store class. 

```ts
setCount(num: number) {
    this.setState({
        ...this.state, count: num,
        history: [...this.state.history, num]
    })
}

```

## Register and Create Store

You finished to define store,
call createProvider and specify ```YourStore``` to stores as array.

```ts
export const provider = () => createProvider({
    stores: [SimpleStore],
})
```

## With React App

memento.react includes a ```<StoreProvider />``` component, which makes the store available to the rest of your app.
Specify provider context to ```StoreProvider``` as react context provider. 

```tsx
import { 
    StoreProvider,
} from "memento.react"
import React,  {useState } from "react"
import {SimpleStoreView} from "./SimpleStoreView"

const App = () => {
    const [provider] = useState(() => provider())

    return (
        <StoreProvider provider={provider}>
            <SimpleStoreView />
        </StoreProvider>
    )
}

```

### Hooks​

memento.react provides a pair of custom React hooks that allow your React components to interact with the store.
class api is not supported.

```useObserver``` reads a value from the store state and subscribes to updates,
 while ```useDispatch``` returns the store's dispatch method to let you dispatch actions defined in store.

```tsx
import { 
    useObserver,
    useDispatch,
} from "memento.react"
import React,  {useState } from "react"

export const SimpleStoreView = () => {dedux
    // store state
    const counter = useObserver(SimpleStore)
    const dispatch = useDispatch(SimpleStore)
    // store state with sliced by selector
    const history = useObserver(SimpleStore, state => state.history)

    // local state
    const [toAdd, setToAdd] = useState(1)

    const handleClick = () => {
        dispatch(store => store.countUpAsync())
    }

    const handleSet = () => {
        dispatch(store => store.setCount(counter.count + toAdd))
        setToAdd(1)
    }

    return (
        <div style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center"
        }}>
            <h2>Counter</h2>

            
            <p>{counter.count}</p>

            <button onClick={handleClick}>
                Count UP
            </button>

            <div style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "260px"
            }}>
                <p>{counter.count}</p>
                <div style={{ flex: "1 1 auto" }} />
                <p>+</p>
                <div style={{ flex: "1 1 auto" }} />
                <input
                    type="number"
                    value={toAdd}
                    onChange={e => setToAdd(Number(e.target.value))}
                />
                <button onClick={handleSet}>
                    Calc
                </button>
            </div>

            <h4>Loading : {counter.isLoading ? "YES" : "NO"}</h4>

            <h4>History</h4>
            <div
                style={{
                    height: "300px",
                    width: "400px",
                    overflowY: "auto",
                    background: "rgba(0,0,0,0.08)",
                    textAlign:"center",
                }}
            >
                {
                    !history.length && <p>No history</p>
                }
                {
                    history.map(h =>
                        <p key={h}>history: {h}</p>
                    )
                }
            </div>
        </div>
    )
}
```

### Other hooks

```useRef``` always refelences currrent state.

Example
```ts
const ref = useRef(SimpleStore)

useEffect(( => {
    console.log(ref.state) // output current state
}), [anyState1, anyState2])
```

```useStore``` provides the store.

Example
```ts
const simpleStore: SimpleStore = useStore(SimpleStore)

useEffect(( => {
    simpleStore.invokeAnyAction() // output current state
}), [])
```

```useStore``` provides a store provider of app.

Example
```ts
const provider = useProvider()

useEffect(( => {
    console.log(provider.rootState()) // output root state
}), [])
```

# API Refelences

[API Refelences](./API.md)

# NEXT

Next, let's see more larger and complex pattern with FluxStore.

[FluxStore](./FluxStore.md)
