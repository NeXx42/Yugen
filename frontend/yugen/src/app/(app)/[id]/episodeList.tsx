"use client"

import * as api from "@lib/api.local"
import React, { Dispatch, SetStateAction, useEffect, useState } from "react";

import "./episodeList.css"
import { MediaEpisodeInfo, MediaInfo } from "@/app/shared/types";

interface Props {
    mediaInfo: MediaInfo,
    setSelectedItem: Dispatch<SetStateAction<MediaEpisodeInfo | undefined>>,
}

const daysOfTheWeek = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
];

const monthsOfThYear = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December"
]

export default function (props: Props) {
    const [episodes, setEpisodes] = useState<MediaEpisodeInfo[]>([]);
    const [episodeReleaseTime, setEpisodeReleaseTime] = useState<number | null>(null);

    const [timeUntil, setTimeUntil] = useState("")
    const [selectedEpisodeIndex, setSelectedEpisodeIndex] = useState<number | null>(null);


    useEffect(() => {
        fetchEpisodes(false);
        api.catalog_EpisodeUpcomingTime(props.mediaInfo.id).then(setEpisodeReleaseTime);

    }, [props.mediaInfo])

    useEffect(() => {
        if (episodeReleaseTime == null) return;

        const update = () => {
            const ms = episodeReleaseTime! * 1000 - Date.now();

            const seconds = Math.floor(ms / 1000) % 60;
            const minutes = Math.floor(ms / (1000 * 60)) % 60;
            const hours = Math.floor(ms / (1000 * 60 * 60)) % 24;
            const days = Math.floor(ms / (1000 * 60 * 60 * 24));

            setTimeUntil(`${days}d ${hours}h ${minutes}m ${seconds}s`);
        };

        update();

        const interval = setInterval(update, 1000);
        return () => clearInterval(interval);
    }, [episodeReleaseTime])

    useEffect(() => {
        var bestTime = 0;
        var bestIndex = null;

        episodes.forEach((e, i) => {
            if ((e.watchDate ?? -1) > bestTime) {
                bestTime = e.watchDate!;
                bestIndex = i;
            }
        })

        if (bestIndex != null) {
            if (episodes[bestIndex].watchPercentage! >= .95 && bestIndex + 1 < episodes.length)
                bestIndex++;
        }
        else if (episodes.length > 0) {
            bestIndex = 0;
        }

        onSelectEpisode(bestIndex);

    }, [episodes])

    const onSelectEpisode = (pos: number | null) => {
        setSelectedEpisodeIndex(pos);
        props.setSelectedItem(pos == null ? undefined : episodes[pos]);
    }

    const fetchEpisodes = (recache: boolean) => {
        api.library_GetEpisodes(props.mediaInfo.id, recache).then(r => setEpisodes(r.sort((a, b) => a.number - b.number)));
    }

    const drawEpisode = (ep: MediaEpisodeInfo, pos: number): React.ReactNode => {
        const watchPercentage = (ep?.watchPercentage ?? 0) * 100;

        return (
            <div className="Episode" key={ep.number} >
                <button className={selectedEpisodeIndex == pos ? "Selected" : ""} onClick={() => onSelectEpisode(pos)}>
                    <div style={{ width: `${watchPercentage}%` }} className="Episode_WatchPercentage" />
                    <a>{`${ep.number}. ${ep.title}`}</a>
                </button>
                {
                    ep.jellyfinId != undefined && (
                        <button className="Episode_Downloaded">
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                                <path d="M14 2v6h6" />
                                <path d="M9 15l2 2 4-4" />
                            </svg>
                        </button>)
                }
            </div>
        )
    }

    const date = props.mediaInfo.startDate ? new Date(props.mediaInfo.startDate * 1000) : undefined;
    if (props.mediaInfo.startDate != null) {
        console.log(date);
    }

    return (
        <div className="EpisodeList ViewPageContainer">
            {
                props.mediaInfo.status === "NOT_YET_RELEASED" ? (
                    <div className="EpisodeList_Unaired">
                        {
                            date != null ?
                                (
                                    <>
                                        <span>PREMIERE</span>
                                        <span>{monthsOfThYear[date?.getMonth()]}</span>
                                        <h1>{date?.getDay()}</h1>
                                        <span>{daysOfTheWeek[date?.getDate()]} · {date?.getFullYear()}</span>
                                    </>
                                ) : (
                                    <>Unkown</>
                                )
                        }
                    </div>
                ) : (<>
                    <div className="EpisodeList_Titlebar">
                        <h2>Episodes</h2>
                        <button onClick={() => fetchEpisodes(true)}>refetch</button>
                    </div>
                    <div className="EpisodeList_Entries">
                        {episodes?.map(drawEpisode)}
                    </div>
                    {
                        episodeReleaseTime != null && (
                            <div className="EpisodeList_Upcoming">
                                {timeUntil}
                            </div>
                        )
                    }
                </>)}
        </div>
    )
}