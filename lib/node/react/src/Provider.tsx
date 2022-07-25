import React from "react";
import { Provider } from "memento.core";
import { StoreContext } from "./StoreContext";

interface ProviderProps {
    provider: Provider;
    children: React.ReactNode;
}

export function StoreProvider({ children, provider }: ProviderProps) {
    return (
        <StoreContext.Provider value={provider}>
            {children}
        </StoreContext.Provider>
    );
}
