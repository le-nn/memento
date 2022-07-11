import "reflect-metadata";
import { createProvider, devToolMiddleware } from "memento.react";
import { LiteAsyncCounterStore } from "./LiteAsyncCounterStore";
import { FluxCounterStore } from "./FluxCounterStore";
import { FluxAsyncCounterStore } from "./FluxAsyncCounterStore";
import { LiteCounterStore } from "./LiteCounterStore";
import { FibonacciCounterStore, FibonacciService } from "./FibonacciCounterStore";

// import { CounterState, CounterStore } from "./counter";
// import { FibonacciService, FibStore } from "./fibonacci";

// const delay =
//     (timeout: number) => new Promise(resolve => setTimeout(resolve, timeout));

// const [countUp, isCountUp, getPayload] = defineMessage<number>("countUp");

// export const HogeStore = defineStore({
//     name: "HogeStore",
//     initialState: () => ({ hoge: 0 }),
//     actions: ({ mutate, getState }) => {
//         return {
//             countUp: async () => {
//                 await delay(1000);
//                 const s = getState();
//                 mutate(countUp(s.hoge + 1));
//             }
//         }
//     },
//     mutation: (s, m) => {
//         switch (true) {
//             case isCountUp(m): {
//                 const payload = getPayload(m);
//                 return {
//                     ...s,
//                     hoge: payload
//                 }
//             }
//         }

//         throw new Error("The message is not handled.");
//     },
// });

export const provider = createProvider({
    stores: [
        FluxCounterStore,
        LiteAsyncCounterStore,
        FluxAsyncCounterStore,
        LiteCounterStore,
        FibonacciCounterStore,
    ],
    middlewares: [
        devToolMiddleware({})
    ],
    services: [
        FibonacciService,
    ]
});

