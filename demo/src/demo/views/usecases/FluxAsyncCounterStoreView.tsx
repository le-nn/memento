import { useDispatch, useObserver } from "memento.react";
import React from "react"
import { useState } from "react";
import { FluxAsyncCounterStore } from "../../stores/FluxAsyncCounterStore";

export const FluxAsyncCounterStoreView = () => {
    // store state
    const counter = useObserver(FluxAsyncCounterStore);
    const dispatch = useDispatch(FluxAsyncCounterStore);
    // store state with sliced by selector
    const history = useObserver(FluxAsyncCounterStore, state => state.history);

    // local state
    const [toAdd, setToAdd] = useState(1);

    const handleClick = () => {
        dispatch(store => store.countUpAsync());
    }

    const handleSet = () => {
        dispatch(store => store.setCount(counter.count + toAdd));
        setToAdd(1);
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

            <button onClick={handleClick}>
                Count UP
            </button>

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
                <button onClick={handleSet}>
                    Calc
                </button>
            </div>

            <h4 style={{ margin: 0, marginTop: "20px" }}>
                Loading : {counter.isLoading ? "YES" : "NO"}
            </h4>

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
                    history.map(h =>
                        <p key={h} style={{ margin: 0, marginTop: "8px" }}>history: {h}</p>
                    )
                }
            </div>
        </div>
    )
}
