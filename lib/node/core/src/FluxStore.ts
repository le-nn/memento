import { constructor } from "./constructor";
import { Message } from "./Message";
import { Observer } from "./Observer";
import { MutationMethod } from "./MutationMethod";
import { StateChangedEventArgs } from "./StateChangedEventArgs";
import { Subscription } from "./Subscription";
import { Provider } from "./Provider";

export class ForceReplace<TState> extends Message<TState> { }

/**
 * Abstract implementation that provides state management immutable with message pattern.
 * "FluxStore" has a state the you want to manage.
 * State is immutable so you should generate new state from "Message" in "mutation" to mutate state.
 * In order to dispatch "mutation", you should invoke "mutate()" from action method defined in sub class.
 *
 * @type {TState} The state you want to manage.
 * @type {TMessage} Possible message type.
 * @example
 * class ExampleState extends State<CounterState> {
 *     count = 0;
 *     message = "text";
 * }
 *
 * class Increment extends Message { }
 * class ModifyText extends Message<string> { }
 * type Messages = Increment | ModifyText
 *
 * class ExampleStore extends FluxStore<ExampleState, Messages> {
 *     constructor() {
 *         super(new ExampleState(), ExampleStore.mutation)
 *     }
 *
 *     static mutation(state: ExampleState, message: Messages) {
 *         switch (message.comparer) {
 *             case Increment:
 *                 return state.clone({ count: state.count + 1 })
 *             case ModifyText:
 *                 const { payload } = message as ModifyText
 *                 return state.clone({ count: state.count + 1 })
 *             default: new Error("message is not handled")
 *         }
 *     }
 *
 *     setText(text: string) {
 *         this.mutate(new ModifyText(text))
 *     }
 *
 *     countUp() {
 *         this.mutate(new Increment())
 *     }
 * }
 *
 */
export abstract class FluxStore<TState = object, TMessage extends Message = Message> {
    private _state: TState;
    private _observers: Observer<TState, TMessage>[] = [];
    private readonly _mutation: MutationMethod<TState, TMessage>;
    private _provider: null | Provider = null;

    // For the storeName without decorator
    public static storeName = "";
    // For the DI without decorator
    public static parameters: constructor<any>[] = [];

    public isTrace = false;

    protected get provider() {
        return this._provider;
    }

    /**
     * The current state.
     */
    public get state(): TState {
        return this._state;
    }

    /**
     * The store name.
     */
    public get storeName() {
        return (this.constructor as any).storeName || this.constructor.name;
    }

    /**
     * Initializes a new instance of FluxStore.
     * @param initialState The initial state.
     * @param mutation The mutation function and preferably static method or pure function.
     */
    constructor(initialState: TState, mutation: MutationMethod<TState, TMessage>) {
        this._state = initialState;
        this._mutation = (state, message) => {
            return mutation(state, message);
        }
    }

    /**
     * Subscribe state changed events.
     * @param observer The handler.
     */
    public subscribe(observer: (e: StateChangedEventArgs<TState, TMessage>) => void): Subscription {
        this._observers = [...this._observers, observer];

        return {
            dispose: () => {
                this._observers = this._observers.filter(o => o !== observer);
            }
        };
    }

    private __setStateForce(state: TState) {
        const message = new ForceReplace<TState>(state) as any;
        const previous = this._state;
        this._state = state;
        this._invokeObserver(previous, state, message);
    }

    private __setStateForceSilently(state: TState) {
        this._state = state;
    }

    private __internal_onInitialized(provider: Provider) {
        this._provider = provider;
        this.onInitialized(provider);
    }

    /**
     * Called on the store initialized.
     */
    protected onInitialized(provider: Provider) {

    }

    /**
     * Change and update state with message for mutate.
     */
    protected mutate(message: TMessage): void {
        const previous = this._state;

        const postState = this.onBeforeMutate(this._state, message);

        // process middlewares
        const middlewares = this.provider?.middlewares() ?? [];
        const middlewareProcessedState = middlewares.reduce(
            (before, middleware) => (s: any, m: Message) => middleware.handle(s, m, before),
            (s: any, m: Message) => this._mutation(s, m as TMessage) as any
        )(postState, message);
        if (!middlewareProcessedState) {
            return;
        }

        this._state = middlewareProcessedState;
        this._state = this.onAfterMutate(middlewareProcessedState, message);

        this._invokeObserver(previous, this._state, message, this.trace());
    }

    /**
     * Called before invoke mutation and return value overrides current state.
     * @param state The current state.
     * @param message The message for mutate state.
     * @returns override state.
     */
    protected onBeforeMutate(state: TState, message: TMessage): TState {
        return state;
    }

    /**
      * Called ofter invoke mutation and return value overrides computed state.
      * @param state The computed state via mutation.
      * @param message The message for mutate state.
      * @returns override state.
      */
    protected onAfterMutate(state: TState, message: TMessage): TState {
        return state;
    }

    /**
     * Invoke state changed event.
     * @param previous The state before state changed.
     * @param state The new state.
     * @param message The message for mutate state.
     */
    private _invokeObserver(previous: TState, state: TState, message: TMessage, trace?: string | null) {
        for (const observer of this._observers) {
            observer({
                message: message,
                store: this.storeName,
                present: state,
                previous: previous,
                timestamp: new Date().getTime(),
                stacktrace: trace ?? "",
            })
        }
    }

    private trace() {
        const stack = this.provider?.tracer();
        if (!stack?.length) {
            return null
        }

        return stack.join("\r\n");
    }
}
