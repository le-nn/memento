import "reflect-metadata";
import { LiteStore } from "./LiteStore";
import { State } from "./State";
import { createProvider } from "./createProvider";
import {
    Injectable,
    Inject
} from "injection-js";
export * from "./constructor";
import { defineStore, defineMessage } from "./functionalWrapper";
import { constructor } from "./constructor";
import { FluxStore, ForceReplace } from "./FluxStore";
import { Provider } from "./Provider";
import { Message } from "./Message";
import { meta } from "./meta";
import { StateChangedEventArgs } from "./StateChangedEventArgs";
import { Middleware } from "./Middleware";
import { devToolMiddleware, openDevTool } from "./devtools/ReduxDevToolMiddleware";

const service = () => {
    return Injectable();
}

const getPayload = <T extends Message>(message: Message): T["payload"] => message.payload;

export type {
    StateChangedEventArgs,
    constructor,
}

export {
    createProvider,
    Inject,
    Provider,
    ForceReplace,
    Middleware,

    // Class API
    LiteStore,
    FluxStore,
    State,
    Message,
    meta,
    service,

    // Functional API
    // getPayload,
    // defineStore,
    // defineMessage

    // middlewares
    devToolMiddleware,
    openDevTool,
};

