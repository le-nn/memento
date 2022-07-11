import { Injectable } from "injection-js";
import { Message } from "./Message";
import { FluxStore } from "./FluxStore";

export const meta = ({ name }: { name: string }): any => {
    return ((ctor: any) => {
        if (ctor.prototype instanceof FluxStore) {
            ctor.storeName = name;
        }
        else if (ctor.prototype instanceof Message) {
            ctor.message = name;
        }
        else {
            throw new Error("@meta() decorator class must extends FluxStore or Message class.");
        }

        return Injectable()(ctor);
    }) as any;
}