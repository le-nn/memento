import { useDispatch, useObserver } from "memento.react";
import React from "react"
import { useState } from "react";
import { FibonacciCounterStore } from "../../stores/FibonacciCounterStore";
import { FluxCounterStore } from "../../stores/FluxCounterStore";

export const FibonacciCounterWithDIView = () => {
    // store state
    const counter = useObserver(FibonacciCounterStore);
    const dispatch = useDispatch(FibonacciCounterStore);

    const handleCalcClick = () => {
        dispatch(store => store.calc());
    }

    return (
        <div
            style={{
                marginTop: "28px",
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                width: "100%",
                height: "100%",
            }}
        >
            <h2 style={{ margin: 0, marginTop: "20px" }}>
                Fibonacci Counter with DI
            </h2>

            <p style={{ margin: 0, marginTop: "20px" }}>
                The most simple usecase is as a store container.<br />
                This is not supported observe detailed with message pattern.<br />
                Looks very simple and easy to handle,<br />
                but as it larger and becomes more complex, it can break the order.<br />
            </p>

            <p>{counter.count}</p>

            <button onClick={handleCalcClick}>
                Calc
            </button>

            <h4 style={{ margin: 0, marginTop: "20px" }}>
                History
            </h4>

            <div
                style={{
                    marginTop: "20px",
                    width: "400px",
                    overflowY: "auto",
                    textAlign: "center",
                    background: "rgba(0,0,0,0.08)",
                    flex: "1 1 auto",
                }}
            >
                {
                    !history.length && <p>No history</p>
                }
                {
                    counter.history.map(h =>
                        <p key={h.n} style={{ margin: 0, marginTop: "8px" }}>
                            n: {h.n} fibonacci: {h.value}
                        </p>
                    )
                }
            </div>
        </div>
    )
}
