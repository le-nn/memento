# Dependency Injection

Memento has a DI container.
DI is an abbreviation for Dependency Inject.
Services implemented as side effects such as 
HTTP Requests
asynchronous,
DB access,
and algorithm implementation 
can be accessed from actions using dependency injection.
It makes to be able to split any features.


## What's DI ?

dependency injection is a design pattern in which an object receives other objects
that it depends on. A form of inversion of control, 
dependency injection aims to separate the concerns of constructing objects and using them, 
leading to loosely coupled programs.

Dependency injection is basically providing the objects that an object needs (its dependencies) instead of having it construct them itself. It's a very useful technique for testing, since it allows dependencies to be mocked or stubbed out.

## Assignment decorator

Assignments ```@service``` decorator to your class.
After that, just specify the type in the constructor argument and it will be assigned automatically without doing anything special.

```ts
@service
class AnyService {
    ...
}
```

## Register to provider

You must register a Service to ```option.services``` when creating a store instance.

```ts
const provider = () => createProvider({
    stores: [...],
    services: [
        FibonacciService,
        HogeService,
        EtcService,
    ]
})
```

## Overview

Create a service class that generate fibonacci number.

```ts
import { Store, service, Message } from "memento.core"

const fibState = {
    n: 0,
    count: 0,
    history: [] as number[]
}

type FibState = typeof fibState

@service()
export class FibonacciService {
    public fib(n: number): number {
        if (n < 3) return 1
        return this.fib(n - 1) + this.fib(n - 2)
    }
}

class SetFib extends Message<number> { }

@store({ name: "fib" })
export class FibStore extends Store<FibState> {
    constructor(readonly fibService: FibonacciService) {
        super(fibState, FibStore.update)
    }

    static mutation(state: FibState, message: Message): FibState {
        switch (message.comparer) {
            case  SetFib: 
                const { payload } = message as SetFib
                return {
                    ...state,
                    n: state.n + 1,
                    count: payload,
                    history: [...state.history, payload]
                }
            default: return state
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
```

When you asignment a type to the constructor args, the library will automatically inject the service.
The build setting of ``` experimentalDecorators``` and ``` emitDecoratorMetadata```emits meta data and makes this possible.

 ```experimentalDecorators``` and ```emitDecoratorMetadata``` 

```ts
export class FibStore extends Store<FibState> {
    constructor(readonly fibService: FibonacciService) {
        super(fibState, FibStore.update)
    }
}
```

## Nested Service

```ts
import { service, meta, FluxStore } from "memento.core"

@service()
class FooService {
    async invoke() {
        return ...
    }
}

@service()
class HogeService {
    constructor(readonly fooService: FooService){}

    async call() {
        return await this.fooService.invoke()
    }
}

class SetTest extends Message<...> {}

@meta({ name: "TestStore" })
class TestStore extends FluxStore<...> {
    constructor(readonly hogeService: HogeService) {
        ...
    }

    async invoke() {
        const result = await this.hogeService.call()
        this.mutate(new SetTest(result))
    }
}
```

### Resolve without Decorator

If you define a static property ```parameters``` that return definition array to inject a dependency, the service will be automatically assigned to constructor arguments when you dispatch the action. Services can also be nested. 
parameters must match the constructor arguments exactly.

```ts
class FooService {
    async invoke() {
        return ...
    }
}

class HogeService {
    static parameters = [FooService]
    constructor(readonly fooService: FooService){}

    async call() {
        return await this.fooService.invoke()
    }
}

class SetTest extends Message<{ 
    test: string 
}> { }

class TestStore extends FluxStore<...> {
    static parameters = [HogeService]

    constructor(readonly hogeService: HogeService) {
        ...
    }

    async invoke() {
        const result = await this.hogeService.call()
        this.mutate(new SetTest(result))
    }
}
```

# Next

[API Refelences](./API.md)
