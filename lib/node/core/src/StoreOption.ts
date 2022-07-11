import { constructor } from "./constructor";
import { FluxStore } from "./FluxStore"
import { Middleware } from "./Middleware"

export interface StoreOption {
    stores: constructor<FluxStore<any>>[];
    middlewares?: constructor<Middleware>[];
    services?: constructor<object>[];
}
