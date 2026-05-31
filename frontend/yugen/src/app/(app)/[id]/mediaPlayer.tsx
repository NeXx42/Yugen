"use client"

import * as api from "@lib/api.local"

import { ReactNode, SubmitEvent, useEffect, useRef, useState } from "react"
import { MediaEpisodeInfo, MediaInfo, Playback_Info } from "@shared/types"

import "./mediaPlayer.css"

import PlayerControl from "@/app/components/playerControls/playerControl"
import { SelectedEpisodeInfo } from "./mediaContainer"


export default function ({ selectedEpisode }: { selectedEpisode: SelectedEpisodeInfo | undefined }) {
    const thumbnail = selectedEpisode?.episodeInfo?.thumbnail ?? selectedEpisode?.mediaInfo?.thumbnailImage;
    const [isPlaying, setIsPlaying] = useState(false);

    useEffect(() => setIsPlaying(false), [selectedEpisode])

    return (
        <div className="MediaPlayer">
            <div className="MediaPlayer_Container">
                {selectedEpisode?.downloadInfo != undefined && isPlaying ? (
                    <PlayerControl mediaInfo={selectedEpisode.mediaInfo} episodeInfo={selectedEpisode.episodeInfo} playbackInfo={selectedEpisode.downloadInfo} />
                ) :
                    (
                        <div className="MediaPlayer_Container_Request" onClick={() => setIsPlaying(true)}>
                            {thumbnail != undefined && <img src={thumbnail ?? ""} />}

                            {
                                selectedEpisode?.downloadInfo?.jellyfinId == undefined ? (
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="1em"
                                        height="1em"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        stroke="currentColor"
                                        strokeWidth="2"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                    >
                                        {/* Arrow */}
                                        <path d="M12 3v12" />
                                        <path d="M7 10l5 5 5-5" />

                                        {/* Tray */}
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
            </div>
        </div >
    )
}