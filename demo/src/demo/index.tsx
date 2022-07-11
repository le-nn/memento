import React, { useEffect, useRef, useState } from "react";
import { StoreProvider } from "memento.react";
import { provider } from "../demo/stores";
import { History } from "./views/History";
import { FluxCounterStoreView } from "./views/usecases/FluxCounterStoreView";
import { CSSProperties } from "react";
import "./style.css"
import { LiteAsyncCounterStoreView } from "./views/usecases/LiteAsyncCounterStoreView";
import { FluxAsyncCounterStoreView } from "./views/usecases/FluxAsyncCounterStoreView";
import { LiteCounterStoreView } from "./views/usecases/LiteCounterStoreView";
import { FibonacciCounterWithDIView } from "./views/usecases/FibonacciCounterWithDIView";

export const Main = () => {
    return (
        <StoreProvider provider={provider}>
            <App />
        </StoreProvider>
    )
}

const views = [
    {
        index: 0,
        name: "Flux Counter",
        view: <FluxCounterStoreView key={0} />
    },
    {
        index: 1,
        name: "Lite Counter",
        view: <LiteCounterStoreView key={1} />
    },
    {
        index: 2,
        name: "Flux Async Counter",
        view: <FluxAsyncCounterStoreView  key={2}/>
    },
    {
        index: 3,
        name: "Lite Async Counter",
        view: <LiteAsyncCounterStoreView key={3} />
    },
    {
        index: 4,
        name: "Fibonacci Counter with DI",
        view: <FibonacciCounterWithDIView  key={4}/>
    },

]

const App = () => {
    const [current, setCurrent] = useState(0)
    return (
        <div style={{
            display: "flex",
            width: "100vw",
            height: "100vh",
            overflowX: "hidden",
            overflowY: "auto"
        }}>
            <div style={{
                width: "100%",
                height: "100%",
                padding: "20px",
                boxSizing: "border-box",
                display: "flex",
                flexDirection: "column",
                minHeight: "900px"
            }}>
                <div style={{ width: "100%" }}>
                    {
                        views.map(v => <button
                            onClick={e => setCurrent(v.index)}
                            key={v.index}
                            style={current === v.index ? linkActive : link}
                        >
                            {v.name}
                        </button>)
                    }
                </div>
                {
                    views.map(v => current === v.index && v.view)
                }
            </div>
            <div
                style={{
                    width: "680px",
                    padding: "20px",
                    boxSizing: "border-box"
                }}
            >
                <History />
            </div>
        </div>
    )
}

const linkActive: CSSProperties = {
    border: 0,
    borderBottom: "2px solid blue",
    background: "transparent",
};

const link: CSSProperties = {
    border: 0,
    borderBottom: 0,
    background: "transparent",
};
