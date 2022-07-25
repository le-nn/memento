import React from "react";
import { Provider } from "memento.core";

export const StoreContext = React.createContext<Provider | null>(null);
