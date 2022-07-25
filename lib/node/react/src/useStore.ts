import { useContext } from "react";
import { constructor, Provider } from "memento.core";
import { StoreContext } from "./StoreContext";
import { useProvider } from "./useProvider";

export const useStore = <T>(type: constructor<T>): T => {
    const provider = useProvider();
    return provider.resolve(type);
};
