"use client"


import * as api from "@lib/api.local"
import { useToast } from "@context/toast";

import { MediaInfo } from "@/app/shared/types";
import { ReactNode, useState } from "react";
import { createPortal } from "react-dom";

import "./seriesControls.css"

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {

    const [isMenuOpen, setMenuOpen] = useState(false);

    const { showToast } = useToast();
    const [bookmarkId, setBookmarkId] = useState(mediaInfo.bookmark);

    const changeBookmarkId = (val: string) => {
        const newBookmarkId = Number.parseInt(val) ?? 0;

        api.library_UpdateBookmark(mediaInfo.id, newBookmarkId).then(() => {
            setBookmarkId(newBookmarkId);
            showToast("Updated bookmark");
        }).catch(() => showToast("Failed to update", "Error"))
    }

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
            <select value={bookmarkId ?? 0} onChange={e => changeBookmarkId(e.target.value)}>
                <option value={0}>None</option>
                <option value={1}>Watching</option>
                <option value={2}>On Hold</option>
                <option value={3}>Planning</option>
                <option value={4}>Completed</option>
                <option value={5}>Dropped</option>
            </select>
            {drawMenu()}
        </>
    )
}