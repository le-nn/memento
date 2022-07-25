import { useDispatch, useObserver } from "memento.react";
import React from "react"
import { useState } from "react";
import { FluxCounterStore } from "../../stores/FluxCounterStore";
import { LiteCounterStore } from "../../stores/LiteCounterStore";

export const LiteCounterStoreView = () => {
    // store state
    const counter = useObserver(LiteCounterStore);
    const dispatch = useDispatch(LiteCounterStore);

    // local state
    const [toAdd, setToAdd] = useState(1);

    const handleIncrementClick = () => {
        dispatch(store => store.increment());
    }

    const handleDecrementClick = () => {
        dispatch(store => store.decrement());
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
                Counter
            </h2>

            <p style={{ margin: 0, marginTop: "20px" }}>
                The most simple usecase is as a store container.<br />
                This is not supported observe detailed with message pattern.<br />
                Looks very simple and easy to handle,<br />
                but as it larger and becomes more complex, it can break the order.<br />
            </p>

            <p>{counter.count}</p>

            <div style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                width: "260px"
            }}>
                <p>{counter.count}</p>
                <div style={{ flex: "1 1 auto" }} />
                <p>+</p>
                <div style={{ flex: "1 1 auto" }} />
                <input
                    type="number"
                    value={toAdd}
                    onChange={e => setToAdd(Number(e.target.value))}
                />
            </div>

            <div style={{ display: "flex" }}>
                <button onClick={handleDecrementClick}>
                    Decrement
                </button>
                <button
                    style={{ marginLeft: "8px" }}
                    onClick={handleIncrementClick}
                >
                    Increment
                </button>
            </div>
        </div>
    );
}
