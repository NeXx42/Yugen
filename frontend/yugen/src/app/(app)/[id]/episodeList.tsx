"use client"

import * as api from "@lib/api.local"

import React, { Dispatch, SetStateAction, useEffect, useState } from "react";
import { EpisodeInfo } from "./mediaContainer"

import "./episodeList.css"

interface Props {
    episodes: EpisodeInfo[] | undefined,
    upcomingEpisode: number | null,

    selectedItem: number | undefined,
    setSelectedItem: Dispatch<SetStateAction<number | undefined>>,
}

export default function (props: Props) {
    const [timeUntil, setTimeUntil] = useState("")

    useEffect(() => {
        if (props.upcomingEpisode == null) return;

        const update = () => {
            const ms = props.upcomingEpisode! * 1000 - Date.now();

            const seconds = Math.floor(ms / 1000) % 60;
            const minutes = Math.floor(ms / (1000 * 60)) % 60;
            const hours = Math.floor(ms / (1000 * 60 * 60)) % 24;
            const days = Math.floor(ms / (1000 * 60 * 60 * 24));

            setTimeUntil(`${days}d ${hours}h ${minutes}m ${seconds}s`);
        };

        update();

        const interval = setInterval(update, 1000);
        return () => clearInterval(interval);
    }, [props.upcomingEpisode])

    const onSelectEpisode = (pos: number) => {
        props.setSelectedItem(props.episodes![pos].episode.number);
    }

    const drawEpisode = (ep: EpisodeInfo, pos: number): React.ReactNode => {
        const watchPercentage = (ep.watchData?.watchPercentage ?? 0) * 100;

        return (
            <div className="Episode" key={ep.episode.number} >
                <button className={ep.episode.number === props.selectedItem ? "Selected" : ""} onClick={() => onSelectEpisode(pos)}>
                    <div style={{ width: `${watchPercentage}%` }} className="Episode_WatchPercentage" />
                    <a>{`${ep.episode.number}. ${ep.episode.title}`}</a>
                </button>
                {
                    (ep.downloadedData !== undefined) ? (
                        (<></>)
                    ) : (
                        <button >
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                                viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                                <path d="M7 10l5 5 5-5" />
                                <path d="M12 15V3" />
                            </svg>
                        </button>
                    )
                }
            </div>
        )
    }

    return (
        <div className="EpisodeList">
            <div className="EpisodeList_Titlebar">
                <h2>Episodes</h2>
            </div>
            <div className="EpisodeList_Entries">
                {props.episodes?.map(drawEpisode)}
            </div>
            {
                props.upcomingEpisode != null && (
                    <div className="EpisodeList_Upcoming">
                        {timeUntil}
                    </div>
                )
            }
        </div>
    )
}