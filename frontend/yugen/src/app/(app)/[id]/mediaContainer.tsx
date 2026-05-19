"use client"

import { ReactNode, useEffect, useState } from "react";

import * as api from "@lib/api.local"

import { MediaEpisodeInfo, MediaInfo } from "@shared/types";
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";

import "./mediaContainer.css"
import { useToast } from "@context/toast";


export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const { showToast } = useToast();

    const [selectedEpisode, setSelectedEpisode] = useState<MediaEpisodeInfo | undefined>();
    const [bookmarkId, setBookmarkId] = useState(mediaInfo.bookmark);

    const drawEpisodeInfo = (): ReactNode => {
        if (selectedEpisode == undefined)
            return (<></>);

        return (<>
            <h2>{`${selectedEpisode}. ${selectedEpisode.title}`}</h2>
            <a>{selectedEpisode.score}</a>
        </>)
    }

    const changeBookmarkId = (val: string) => {
        const newBookmarkId = Number.parseInt(val) ?? 0;

        api.library_UpdateBookmark(mediaInfo.id, newBookmarkId).then(() => {
            setBookmarkId(newBookmarkId);
            showToast("Updated bookmark");
        }).catch(() => showToast("Failed to update", "Error"))
    }

    const drawBookmark = (): ReactNode => {

        return (
            <select value={bookmarkId ?? 0} onChange={e => changeBookmarkId(e.target.value)}>
                <option value={0}>None</option>
                <option value={1}>Watching</option>
                <option value={2}>On Hold</option>
                <option value={3}>Planning</option>
                <option value={4}>Completed</option>
                <option value={5}>Dropped</option>
            </select>
        )
    }

    return (
        <div className="MediaContainer">
            <div className="MediaContainer_Media">
                <MediaPlayer mediaInfo={mediaInfo} episode={selectedEpisode} bookmarkNode={drawBookmark()} />
                <EpisodeList mediaInfo={mediaInfo} setSelectedItem={setSelectedEpisode} />
            </div>

            <div className="MediaContainer_EpisodeInfo ViewPageContainer">
                <div className="MediaContainer_EpisodeInfo_Left">
                    {drawEpisodeInfo()}
                </div>
            </div>
        </div>
    )
}