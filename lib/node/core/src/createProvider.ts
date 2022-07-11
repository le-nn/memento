import { StoreOption } from "./StoreOption";
import { Provider } from "./Provider";

/**
 * Create a store.
 * @param option store option
 */
export const createProvider = (option: StoreOption) => {
    try {
        return new Provider(option);
    }
    catch (ex: any) {
        console.error(ex.message);
        throw new Error(ex.message);
    }
}
