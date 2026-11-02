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
    const { showModal } = useModals();

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
            <div className="SeriesController_ElementContainerGroup">
                <select className="ViewPage_SeriesControl SeriesControl_Bookmark" value={bookmarkId ?? 0} onChange={e => changeBookmarkId(e.target.value)}>
                    <option value={0}>None</option>
                    <option value={1}>Watching</option>
                    <option value={2}>On Hold</option>
                    <option value={3}>Planning</option>
                    <option value={4}>Completed</option>
                    <option value={5}>Dropped</option>
                </select>
            </div>
            <button className="ViewPage_SeriesControl" onClick={() => openMenu("Requests")}>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M.5 9.9a.5.5 0 0 1 .5.5v2.5a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-2.5a.5.5 0 0 1 1 0v2.5a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-2.5a.5.5 0 0 1 .5-.5" />
                    <path d="M7.646 11.854a.5.5 0 0 0 .708 0l3-3a.5.5 0 0 0-.708-.708L8.5 10.293V1.5a.5.5 0 0 0-1 0v8.793L5.354 8.146a.5.5 0 1 0-.708.708z" />
                </svg>
                <span style={{ marginLeft: 5 }}>Download</span>
            </button>
            <button className="ViewPage_SeriesControl" onClick={() => openMenu("Subtitles")}>Subtitles</button>
        </>
    )
}