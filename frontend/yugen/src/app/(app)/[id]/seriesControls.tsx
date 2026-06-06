"use client"


import * as api from "@lib/api.local"
import { useToast } from "@context/toast";

import { MediaInfo } from "@shared/types";
import { useState } from "react";

import "./seriesControls.css"

import SeriesRequestModal from "./seriesRequestModal";
import MassSubtitleEditor from "./massSubtitleEditor";
import { useModals } from "@/app/context/modalContext";

type MenuType = "None" | "Subtitles" | "Requests";

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const { showToast } = useToast();
    const { showModal, closeModals } = useModals();

    const [bookmarkId, setBookmarkId] = useState(mediaInfo.bookmark);

    const changeBookmarkId = (val: string) => {
        const newBookmarkId = Number.parseInt(val) ?? 0;

        api.library_UpdateBookmark(mediaInfo.id, newBookmarkId).then(() => {
            setBookmarkId(newBookmarkId);
            showToast("Updated bookmark");
        }).catch(() => showToast("Failed to update", "Error"))
    }

    const openMenu = (type: MenuType) => {
        switch (type) {
            case "Requests":
                showModal(<SeriesRequestModal mediaInfo={mediaInfo} onUpdate={() => { }} onClose={() => { }} />)
                break;

            case "Subtitles":
                showModal(<MassSubtitleEditor mediaInfo={mediaInfo} />);
                break;
        }
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
                <button className="ViewPage_SeriesControl" onClick={() => openMenu("Requests")}>+</button>
            </div>
            <button className="ViewPage_SeriesControl" onClick={() => openMenu("Subtitles")}>Subtitles</button>
        </>
    )
}