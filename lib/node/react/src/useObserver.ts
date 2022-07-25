import { useEffect, useReducer, useRef } from "react";
import { useProvider } from "./useProvider";
import { constructor, FluxStore } from "memento.core";

export const useObserver = <
    TState,
    TSelectedState = TState>(
        storeType: constructor<FluxStore<TState>>,
        selector?: (state: TState) => TSelectedState
    ): TSelectedState => {
    const _selector = selector || ((s: TState) => s as any as TSelectedState);
    const [, forceRender] = useReducer((s) => s + 1, 0);
    const provider = useProvider();
    const state = useRef(_selector(provider.resolve(storeType).state));

    useEffect(() => {
        let isMount = true;
        const store = provider.resolve(storeType) as FluxStore<TState>;
        const subscription = store.subscribe(_ => {
            const newState = _selector(store.state);
            if (state.current !== newState && isMount) {
                state.current = newState;
                forceRender();
            }
        });

        return () => {
            subscription.dispose();
            isMount = false;
        };
    }, [provider]);

    return _selector(provider.resolve(storeType).state);
}
