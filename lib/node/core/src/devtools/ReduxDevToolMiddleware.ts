import {
    Message,
    Provider,
    FluxStore,
    ForceReplace,
    StateChangedEventArgs,
    Middleware,
    constructor
} from "..";

import { LiftedStore } from "./LiftedStore"

const _window = typeof window !== "undefined" ? window : {__MEMENTO_DEV_TOOL:{}}

interface DevToolOption {
    name?: string;
    maxAge?: number;
    latency?: number;
    trace?: boolean;
}

export const openDevTool = () => {
    (_window as any).__REDUX_DEVTOOLS_EXTENSION__?.open()
}

// TODO: Reset, Commit, Revert, Sweet
// TODO: Stack tarce optimization
// TODO: Support Reorder

export const devToolMiddleware = (_option?: DevToolOption): constructor<Middleware> => {
    const option = {
        name: "Memento Devtool",
        maxAge: 10,
        latency: 800,
        trace: false,
        ..._option,
    };

    return class ReduxDevToolMiddleware extends Middleware {
        reduxDevTools: any | null = null
        currentSequence = 0
        liftedStore?: LiftedStore
        subscription: null | { dispose: () => void } = null;

        constructor() {
            super();
        }

        public override onInitialized(provider: Provider<{ [key: string]: any; }>): void {
            const stores = provider.storeBag();
            for (const storeKey in stores) {
                stores[storeKey].isTrace = option.trace
            }

            (_window as any).__MEMENTO_DEV_TOOL = this;
            this.reduxDevTools = (_window as any)
                ?.__REDUX_DEVTOOLS_EXTENSION__
                ?.connect({
                    ...option,
                    features: {
                        pause: true, // start/pause recording of dispatched actions
                        lock: true, // lock/unlock dispatching actions and side effects
                        persist: false, // persist states on page reloading
                        export: true, // export history of actions in a file
                        import: 'custom', // import history of actions from a file
                        jump: true, // jump back and forth (time travelling)
                        skip: true, // skip (cancel) actions
                        reorder: true, // drag and drop actions in the history list
                        dispatch: false, // dispatch custom actions or action creators
                        test: false, // generate tests for the selected actions
                    },
                });
            if (!this.reduxDevTools) {
                console.error("failed to connect redux devtool")
            }

            this.liftedStore = new LiftedStore(
                provider,
                provider.rootState(),
                provider.storeBag(),
                this.reduxDevTools
            )

            this.init(provider.rootState())
            this.subscription = provider.subscribe(e => {
                this.send(e, provider.rootState())
            })
        }

        public override handle(
            state: object,
            message: Message,
            next: (state: object, message: Message) => object)
            : object | null {
            const s = next(state, message);
            // Ignore to mutate state while state is being jumped
            if (!this.liftedStore?.canMutate) {
                return null;
            }

            return s;
        }

        public override dispose() {
            (_window as any).__MEMENTO_DEV_TOOL = undefined
            this.subscription?.dispose();
        }

        private init(state: any) {
            this.reduxDevTools?.subscribe((message: any) => {
                switch (message?.payload?.type) {
                    case "COMMIT":
                        break;
                    case "JUMP_TO_STATE":
                    case "JUMP_TO_ACTION":
                        this.liftedStore?.jumpTo(message.payload.actionId)
                        break;
                    case "TOGGLE_ACTION":
                        this.liftedStore?.skip(message.payload.id)
                }
            });
            this.reduxDevTools?.init(state);
        }

        private send(e: StateChangedEventArgs, rootState: any) {
            if (e.message.comparer === ForceReplace) {
                return;
            }

            this.liftedStore?.push(e, rootState)
        }

    }
}
