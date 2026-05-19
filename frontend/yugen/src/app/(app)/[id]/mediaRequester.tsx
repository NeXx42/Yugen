"use client"

import { MediaInfo } from "@/app/shared/types";
import { ReactNode, useState } from "react";
import { createPortal } from "react-dom";

import "./mediaRequester.css"

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const [isMenuOpen, setMenuOpen] = useState(false);

    const drawMenu = (): ReactNode => {
        if (!isMenuOpen)
            return (<></>);

        return createPortal(
            <div className="MediaRequest" onClick={() => setMenuOpen(false)}>
                <div className="MediaRequest_Menu">

                </div>
            </div>
            , document.body)
    }

    return (
        <>
            <button onClick={() => setMenuOpen(true)}>Request</button>
            {drawMenu()}
        </>
    )
}