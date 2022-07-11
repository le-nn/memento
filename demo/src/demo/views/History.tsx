import { useProvider } from "@memento/react";
import React, { useEffect, useRef, useState } from "react";

export const History = () => {
    const provider = useProvider();
    const [histories, setHistories] = useState<any[]>([]);
    const i = useRef(0);

    const ref = useRef<HTMLDivElement>(null)

    useEffect(() => {
        const s = provider.subscribe(e => {
            setHistories([
                ...histories,
                {
                    key: i.current,
                    ...e,
                    time: new Date(),
                    root: provider.rootState()
                }
            ]);
            i.current++;

            const scroll = ref.current;
            if (scroll) {
                scroll.scrollTop = (scroll.scrollHeight - scroll.clientHeight);
            }
        });

        return () => s.dispose();
    }, [histories]);

    return (
        <div style={{
            display: "flex",
            flexDirection: "column",
            width: "100%",
            height: "100%",
        }}>
            <h2>History</h2>
            <div
                ref={ref}
                style={{
                    flex: "1 1 auto",
                    overflowY: "scroll",
                    background: "#f7f7f7",
                    padding: "20px",
                    scrollBehavior: "smooth"
                }}>
                {
                    histories.map((h, i) => (<React.Fragment key={h.key}>
                        <details >
                            <summary>
                                <div style={{ display: "flex" }}>
                                    {`${h.key} ${h.store}`}
                                    <strong style={{ marginLeft: "8px", color: "#11b7af" }}>
                                        {h.message.name}
                                    </strong>
                                    <div style={{ marginLeft: "8px" }}>{toString(h.time)}</div>
                                </div>
                            </summary>
                            <div style={{ whiteSpace: "pre", padding: "16px", background: "#f0f0f0" }}>{JSON.stringify(h.message, null, 4)}</div>
                            <div>State</div>
                            <div style={{ whiteSpace: "pre", padding: "16px", background: "#f0f0f0" }}>{JSON.stringify(h.root, null, 4)}</div>
                        </details>
                        <hr />
                    </React.Fragment >))
                }
            </div>
        </div>
    );
};

const toString = (date: Date) =>
    `${date.getMonth() + 1}/${date.getDate()}/${date.getFullYear()} ${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`;
