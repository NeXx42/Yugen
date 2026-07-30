"use client"

import { ReactNode } from "react";
import "./errorContainer.css"

interface Props {
    error: string | undefined,
    loading: boolean,

    loadingDisplay: ReactNode,
    dataDisplay: ReactNode,

    retryFunc: () => void;
}

export default function (props: Props) {
    if (props.error) {
        return (
            <div className="ErrorContainer">
                {props.error}
                <button onClick={props.retryFunc}>Retry</button>
            </div>
        )
    }

    if (props.loading)
        return props.loadingDisplay;

    return props.dataDisplay;
}