"use client"

import { ReactNode, useEffect, useState } from "react";


import { MediaEpisodeInfo, MediaInfo } from "@shared/types";
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";

import "./mediaContainer.css"
import { createPortal } from "react-dom";



export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    if (mediaInfo.type === "MOVIE") {
        return <MediaPlayer mediaInfo={mediaInfo} episode={undefined} />
    }

    const [episodeContainer, setEpisodeContainer] = useState<HTMLElement | null>(null)
    const [selectedEpisode, setSelectedEpisode] = useState<MediaEpisodeInfo | undefined>();

    const drawEpisodeInfo = (): ReactNode => {
        if (selectedEpisode == undefined)
            return (<></>);

        return (<div className="ViewPageContainer">
            <h2>{`${selectedEpisode.number}. ${selectedEpisode.title}`}</h2>
            <a>{selectedEpisode.score}</a>
        </div>)
    }

    useEffect(() => setEpisodeContainer(document.getElementById("ViewPage_EpisodeInfo")), [])

    return (
        <div className="MediaContainer">
            <div className="MediaContainer_Media">
                <MediaPlayer mediaInfo={mediaInfo} episode={selectedEpisode} />
                <EpisodeList mediaInfo={mediaInfo} setSelectedItem={setSelectedEpisode} />
            </div>

            {selectedEpisode && episodeContainer && createPortal(drawEpisodeInfo(), episodeContainer)}
        </div>
    )
}