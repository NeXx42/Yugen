"use client"


import * as api from "@lib/api.local"
import { useToast } from "@context/toast";

import { MediaInfo } from "@/app/shared/types";
import { ReactNode, useState } from "react";
import { createPortal } from "react-dom";

import "./seriesControls.css"

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const { showToast } = useToast();

    const [isMenuOpen, setMenuOpen] = useState(false);
    const [bookmarkId, setBookmarkId] = useState(mediaInfo.bookmark);

    const [sendingRequest, setSendingRequest] = useState(false);

    const changeBookmarkId = (val: string) => {
        const newBookmarkId = Number.parseInt(val) ?? 0;

        api.library_UpdateBookmark(mediaInfo.id, newBookmarkId).then(() => {
            setBookmarkId(newBookmarkId);
            showToast("Updated bookmark");
        }).catch(() => showToast("Failed to update", "Error"))
    }

    const requestMedia = () => {
        setSendingRequest(true);
        api.library_RequestSeries(mediaInfo.id, 1, "/tv").finally(() => setSendingRequest(false));
    }

    const drawMenu = (): ReactNode => {
        if (!isMenuOpen)
            return (<></>);

        return createPortal(
            <div className="MediaRequest" onClick={() => setMenuOpen(false)}>
                <div className="MediaRequest_Menu">
                    <h1>{mediaInfo.title}</h1>
                    <button onClick={requestMedia}>Request</button>
                </div>
            </div>
            , document.body)
    }

    return (
        <>
            <div className="SeriesControls">
                <select className="ViewPage_SeriesControl SeriesControl_Bookmark" value={bookmarkId ?? 0} onChange={e => changeBookmarkId(e.target.value)}>
                    <option value={0}>None</option>
                    <option value={1}>Watching</option>
                    <option value={2}>On Hold</option>
                    <option value={3}>Planning</option>
                    <option value={4}>Completed</option>
                    <option value={5}>Dropped</option>
                </select>
                <button className="ViewPage_SeriesControl" onClick={() => setMenuOpen(true)}>+</button>
            </div>
            {drawMenu()}
        </>
    )
}