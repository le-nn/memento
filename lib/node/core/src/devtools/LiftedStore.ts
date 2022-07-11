import {
    Message,
    Provider,
    FluxStore,
    ForceReplace,
    StateChangedEventArgs,
    Middleware,
    constructor
} from "..";

interface HistoryState {
    id: number;
    storeBag: { [key: string]: FluxStore };
    message: Message;
    storeName: string;
    rootState: { [key: string]: any };
    stacktrace: string;
    timestamp: number;
}

class Init extends Message { }

interface Action {
    action: {
        type: string,
        payload: any,
    },
    timestamp: number,
    stack: any,
    type: string
}

interface DevToolStateContext {
    actionsById: { [key: number]: Action }
    computedStates: { state: any }[]
    currentStateIndex: number,
    nextActionId: number,
    skippedActionIds: number[],
    stagedActionIds: number[]
}


export class LiftedStore {
    histories: { [key: number]: HistoryState } = {};
    skippedActionIds: number[] = []
    stagedActionIds: number[] = []
    currentCursor = 0;
    sequence = 0;

    get nextActionId() {
        return this.sequence + 1;
    }

    get currentHistory() {
        return this.histories[this.currentCursor]
    }

    get canMutate() {
        return this.currentCursor === this.sequence;
    }

    constructor(readonly provider: Provider,
        readonly rootState: any,
        readonly storeBag: { [key: string]: FluxStore, },
        readonly devTool: any
    ) {
        this.reset()
    }

    reset() {
        this.currentCursor = 0
        this.sequence = 0
        this.stagedActionIds = [0]
        this.skippedActionIds = []
        this.histories = {
            0: {
                message: new Init(),
                storeName: "",
                rootState: this.rootState,
                id: 0,
                storeBag: this.storeBag,
                stacktrace: "",
                timestamp: 0,
            }
        }

    }

    push(e: StateChangedEventArgs, rootState: any) {
        if (this.currentCursor !== this.sequence) {
            return;
        }

        this.currentCursor++;
        this.sequence++;

        this.stagedActionIds = [...this.stagedActionIds, this.sequence]
        this.histories[this.sequence] = {
            id: this.sequence,
            storeBag: this.provider.storeBag(),
            message: e.message,
            storeName: e.store,
            rootState: rootState,
            stacktrace: e.stacktrace ?? "",
            timestamp: e.timestamp,
        }

        this.syncWithPlugin();
    }

    jumpTo(id: number) {
        this.currentCursor = id;
        const history = this.histories[id];
        this.setStatesToStore(history)
    }

    setStatesToStore(history: HistoryState) {
        if (history) {
            for (const storeName in history.storeBag) {
                if (storeName === history.storeName
                    || history.message.comparer === Init
                    || (history.storeBag[storeName] as any).state !== history.rootState[storeName]
                ) {
                    // target store should invoke change event
                    (history.storeBag[storeName] as any).__setStateForce(history.rootState[storeName])
                }
                else {
                    // ignore to invoke change event because update ui is heavy
                    (history.storeBag[storeName] as any).__setStateForceSilently(history.rootState[storeName])
                }
            }
        }
        else {
            throw new Error("")
        }
    }

    skip(id: number) {
        if (this.skippedActionIds.includes(id)) {
            this.skippedActionIds = this.skippedActionIds.filter(x => x !== id);
        }
        else {
            this.skippedActionIds = [...this.skippedActionIds, id];
        }

        this.calcState()
        this.syncWithPlugin()
        this.setStatesToStore(this.currentHistory)
    }

    calcState() {
        const histories = this.histories

        const newHistories: typeof histories = {}

        let beforeState = histories[0].rootState
        if (!beforeState) {
            throw new Error("");
        }

        for (const key in this.histories) {
            const history = histories[key]
            // initial or skipped history
            if (Number(key) === 0 || this.skippedActionIds.includes(history.id)) {
                newHistories[key] = {
                    ...history,
                    rootState: beforeState,
                }
                continue;
            }

            const store = history.storeBag[history.storeName]
            const state = (store as any)._mutation(
                beforeState[history.storeName],
                history.message
            )

            beforeState = {
                ...beforeState,
                [history.storeName]: state,
            }

            newHistories[key] = {
                ...history,
                rootState: beforeState,
            }
        }

        this.histories = newHistories;
    }

    syncWithPlugin() {
        const sended: DevToolStateContext = {
            actionsById: Object.keys(this.histories)
                .map(key => this.histories[Number(key)])
                .reduce((x, y) => ({
                    ...x,
                    [y.id]: {
                        action: y.message,
                        type: "PERFORM_ACTION",
                        stack: y.stacktrace,
                        timestamp: y.timestamp,
                    }
                }), {}),
            computedStates: Object.keys(this.histories)
                .map(key => this.histories[Number(key)])
                .map(history => ({ state: history.rootState })),
            nextActionId: this.nextActionId,
            currentStateIndex: this.currentCursor,
            skippedActionIds: [...this.skippedActionIds],
            stagedActionIds: [...this.stagedActionIds]
        }

        this.devTool.send(null, sended)
    }
}
