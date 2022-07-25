import { constructor, FluxStore } from "memento.core";
import { useProvider } from "./useProvider";

class StateGetter {
    store: FluxStore;

    public get state() {
        return this.store.state;
    }

    constructor(store: FluxStore) {
        this.store = store;
    }
}

export const useStateRef = <T extends FluxStore>(type: constructor<T>): { state: T["state"] } => {
    const provider = useProvider();
    const store = provider.resolve(type);
    return new StateGetter(store);
};
