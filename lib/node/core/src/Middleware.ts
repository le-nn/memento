import { Message } from "Message";
import { Provider } from "./Provider";

export type MiddlewareHandler = (state: object, message: Message) => object;

export abstract class Middleware {
    protected provider?: Provider;

    /**
     * Called on the store initialized.
     */
    public onInitialized(provider: Provider): void {
        this.provider = provider;
    }


    public handle(
        state: object,
        message: Message,
        next: MiddlewareHandler
    ): object | null {
        return next(state, message);
    }

    public dispose() {

    }
}
