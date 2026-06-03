"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useState } from "react";
import { MediaEpisodeInfo, MediaInfo, Playback_Info } from "@shared/types";
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";

import "./mediaContainer.css"
import { createPortal } from "react-dom";
import SubtitleEditor from "./subtitleEditor";

export interface SelectedEpisodeInfo {
    mediaInfo: MediaInfo,
    episodeInfo: MediaEpisodeInfo,
    downloadInfo?: Playback_Info
}

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const getSelectedEpisodeInfo = async (ep: MediaEpisodeInfo | undefined | null): Promise<SelectedEpisodeInfo | undefined> => {
        if (!ep)
            return undefined;

        if (ep?.jellyfinId) {
            const downloadInfo = await api.media_PlaybackInfo(mediaInfo.id, ep.number, ep.jellyfinId);

            return {
                mediaInfo,
                episodeInfo: ep,
                downloadInfo
            }
        }

        return {
            mediaInfo,
            episodeInfo: ep,
        }
    }

    if (mediaInfo.type === "MOVIE") {
        const [filmEpisode, setFileEpisode] = useState<SelectedEpisodeInfo | null>();

        useEffect(() => {
            api.library_GetFilm(mediaInfo.id, false)
                .then(e => getSelectedEpisodeInfo(e)
                    .then(setFileEpisode));

        }, [mediaInfo])

        return <MediaPlayer selectedEpisode={filmEpisode ?? undefined} />
    }

    const [episodeContainer, setEpisodeContainer] = useState<HTMLElement | null>(null)
    const [selectedEpisode, setSelectedEpisode] = useState<SelectedEpisodeInfo | undefined>();

    const updatedSelectedEpisode = (to: MediaEpisodeInfo | undefined) => {
        if (to) {
            getSelectedEpisodeInfo(to).then(setSelectedEpisode);
        }
        else {
            setSelectedEpisode(undefined);
        }
    }


    const drawEpisodeInfo = (): ReactNode => {
        if (selectedEpisode == undefined)
            return (<></>);

        return (<div className="ViewPageContainer MediaContainer_EpisodeInfo">
            <div className="MediaContainer_EpisodeInfo_Left">
                <span>{`${selectedEpisode.episodeInfo.number}. ${selectedEpisode.episodeInfo.title}`}</span>
                <div>
                    <svg viewBox="0 0 24 24" width={14} height={14} fill="currentColor">
                        <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
                    </svg>
                    <a>{(selectedEpisode.episodeInfo.score ?? 0) * 2}</a>
                </div>
            </div>
            <div className="MediaContainer_EpisodeInfo_Right">
                <SubtitleEditor mediaInfo={mediaInfo} episodeInfo={selectedEpisode.episodeInfo} playbackInfo={selectedEpisode.downloadInfo!} onUpdate={() => updatedSelectedEpisode(selectedEpisode?.episodeInfo)} />
            </div>
        </div>)
    }

    useEffect(() => setEpisodeContainer(document.getElementById("ViewPage_EpisodeInfo")), [])

    return (
        <div className="MediaContainer">
            <div className="MediaContainer_Media">
                <MediaPlayer selectedEpisode={selectedEpisode} />
                <EpisodeList mediaInfo={mediaInfo} setSelectedItem={updatedSelectedEpisode} />
            </div>

            {selectedEpisode && episodeContainer && createPortal(drawEpisodeInfo(), episodeContainer)}
        </div>
    )
}