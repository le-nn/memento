import { FluxStore } from "./FluxStore";
import { ReflectiveInjector, Injectable, Injector } from "injection-js";
import { constructor } from "./constructor";
import { Message } from "./Message";
import { StateChangedEventArgs } from "./StateChangedEventArgs";
import { StoreOption } from "./StoreOption";
import { Subscription } from "./Subscription";
import { Middleware } from "Middleware";

type Observer = (e: StateChangedEventArgs, store: FluxStore) => void

type Tracer = () => string;

export class Provider<TRootState = { [key: string]: any }> {
    private readonly _container!: ReflectiveInjector;
    private readonly _storesDefines: { name: string, type: constructor<object> }[];
    private readonly _midlewareDefines: constructor<object>[];
    private observers: Observer[] = [];
    private subscriptions: Subscription[] = [];
    readonly option: StoreOption;

    public _tracers: Tracer[] = [() => "None"];

    public tracer() {
        return this._tracers;
    }

    public addTracer(tracer: Tracer) {
        this._tracers = [...this._tracers, tracer];
    }

    public storeBag(): { [key: string]: FluxStore } {
        return this._storesDefines.reduce((x, y) => ({
            ...x,
            [y.name]: this._container.get(y.type)
        }), {} as { [key: string]: FluxStore });
    }


    public middlewares(): Middleware[] {
        return this._midlewareDefines.map(x => this._container.get(x) as Middleware);
    }

    public rootState(): TRootState {
        return this._storesDefines.reduce((x, y) => ({
            ...x,
            [y.name]: this._container.get(y.type).state
        }), {} as TRootState);
    }

    constructor(option: StoreOption) {
        this.option = { ...option };
        this._container = ReflectiveInjector.resolveAndCreate([
            ...(option.services || []),
            ...(option.middlewares || []),
            ...option.stores
        ]);

        this._midlewareDefines = option.middlewares ?? [];
        this._storesDefines = option.stores.map(c => ({
            name: (c as any).storeName,
            type: c
        }));

        const subscriptions = [];
        const _storeNames: { [key: string]: boolean } = {}
        for (const ctor of option.stores) {
            try {
                const storeName = (ctor as any).storeName
                if (!storeName) {
                    throw new Error(`The Store name is empty. plesase specify Store name. ${ctor.name}`)
                }
                if (_storeNames[storeName]) {
                    throw new Error(`The Store name "${storeName}" is already exists.`)
                }
                _storeNames[storeName] = true

                const store = this._container!.get(ctor) as FluxStore;
                const subscription = store.subscribe((e: StateChangedEventArgs<object>) => {
                    this.invokeObservers(e, store)
                });
                subscriptions.push(subscription);
            }
            catch (ex: any) {
                throw new Error(`Failed to create memento provider "${ex.message}" \n`);
            }
        }
        this.subscriptions = subscriptions;
        // handle middlewares
        for (const ctor of option.middlewares ?? []) {
            try {
                const middleware = this._container!.get(ctor) as Middleware;
                middleware.onInitialized(this as any);
            }
            catch (ex: any) {
                throw new Error(`Failed to initalize memento middleware "${ex.message}" \n`);
            }
        }

        // call onInitialized
        for (const ctor of option.stores) {
            try {
                const store = this._container!.get(ctor) as FluxStore;
                (store as any).__internal_onInitialized(this);
            }
            catch (ex: any) {
                throw new Error(`Failed to initalize memento provider "${ex.message}" \n`);
            }
        }
    }

    public subscribe(observer: Observer): Subscription {
        this.observers = [...this.observers, observer];

        return {
            dispose: () => {
                this.observers = this.observers.filter(o => o !== observer);
            }
        };
    }

    invokeObservers(e: StateChangedEventArgs, store: FluxStore) {
        for (const observer of this.observers) {
            observer(e, store);
        }
    }

    public resolve<T>(type: constructor<T>) {
        return this._container.get(type) as T;
    }

    public dispose() {
        for (const s of this.subscriptions) {
            s.dispose();
        }
        this.subscriptions = [];
    }
}
