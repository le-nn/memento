import React from "react";
import { constructor, FluxStore } from "memento.core";
import { useProvider } from "./useProvider";

type Dispatcher<TStore> = (store: Omit<TStore, keyof FluxStore>) => Promise<void> | void;

export const useDispatch = <TStore extends FluxStore>(storeType: constructor<TStore>) => {
    const provider = useProvider();
    const store = provider.resolve(storeType);
    return async (dispatcher: Dispatcher<TStore>) => {
        const context = dispatcher(store);
        if (context instanceof Promise) {
            await context;
        }
    };
};
