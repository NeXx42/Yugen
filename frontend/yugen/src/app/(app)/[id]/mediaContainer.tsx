"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useState } from "react";
import { MediaEpisodeInfo, MediaInfo, Playback_Info } from "@shared/types";
import EpisodeList from "./episodeList";

import "./mediaContainer.css"
import { createPortal } from "react-dom";
import SubtitleEditor from "./subtitleEditor";
import PlayerControl from "@/app/components/playerControls/playerControl";
import SeriesRequestModal from "./seriesRequestModal";

export interface SelectedEpisodeInfo {
    mediaInfo: MediaInfo,
    episodeInfo: MediaEpisodeInfo,
    downloadInfo?: Playback_Info
}

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const [isPlaying, setIsPlaying] = useState(false);
    const [isManagingMedia, setIsManagingMedia] = useState(false);

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

    const drawMediaPlayer = (selectedEpisode: SelectedEpisodeInfo | undefined) => {
        const thumbnail = selectedEpisode?.episodeInfo?.thumbnail ?? selectedEpisode?.mediaInfo?.thumbnailImage;

        const attemptToPlay = () => {
            if (selectedEpisode?.downloadInfo) {
                setIsPlaying(true);
            }
            else {
                setIsManagingMedia(true);
            }
        }

        return (
            <div className="MediaPlayer_Container">
                {selectedEpisode?.downloadInfo != undefined && isPlaying ? (
                    <PlayerControl mediaInfo={selectedEpisode.mediaInfo} episodeInfo={selectedEpisode.episodeInfo} playbackInfo={selectedEpisode.downloadInfo} />
                ) :
                    (
                        <div className="MediaPlayer_Container_Request" onClick={attemptToPlay}>
                            {thumbnail != undefined && <img src={thumbnail ?? ""} />}
                            {
                                selectedEpisode?.downloadInfo?.jellyfinId == undefined ? (
                                    <svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" >
                                        <path d="M12 3v12" />
                                        <path d="M7 10l5 5 5-5" />
                                        <path d="M5 21h14" />
                                    </svg>
                                ) :
                                    (
                                        <svg stroke="currentColor" fill="currentColor" viewBox="0 0 448 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                                            <path d="M424.4 214.7L72.4 6.6C43.8-10.3 0 6.1 0 47.9V464c0 37.5 40.7 60.1 72.4 41.3l352-208c31.4-18.5 31.5-64.1 0-82.6z"></path>
                                        </svg>
                                    )
                            }
                        </div>
                    )
                }
            </div >
        )
    }

    if (mediaInfo.type === "MOVIE") {
        const [filmEpisode, setFileEpisode] = useState<SelectedEpisodeInfo | null>();

        useEffect(() => {
            api.library_GetFilm(mediaInfo.id, false)
                .then(e => getSelectedEpisodeInfo(e)
                    .then(setFileEpisode));

        }, [mediaInfo])

        return drawMediaPlayer(filmEpisode ?? undefined);
    }

    const [episodeContainer, setEpisodeContainer] = useState<HTMLElement | null>(null)
    const [selectedEpisode, setSelectedEpisode] = useState<SelectedEpisodeInfo | undefined>();

    const updatedSelectedEpisode = (to: MediaEpisodeInfo | undefined) => {
        setIsPlaying(false);
        setIsManagingMedia(false);

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

        return (<div className="ViewPageContainer MediaContainer_EpisodeInfo" style={{ marginBottom: "10px" }}>
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
                {drawMediaPlayer(selectedEpisode)}
                <EpisodeList mediaInfo={mediaInfo} setSelectedItem={updatedSelectedEpisode} />
            </div>

            {selectedEpisode && episodeContainer && createPortal(drawEpisodeInfo(), episodeContainer)}
            {
                isManagingMedia && selectedEpisode?.mediaInfo && <SeriesRequestModal mediaInfo={selectedEpisode.mediaInfo} onUpdate={() => { }} onClose={() => setIsManagingMedia(false)} />
            }
        </div>
    )
}