import React from "react";
import { Provider } from "memento.js";

export const StoreContext = React.createContext<Provider | null>(null);
