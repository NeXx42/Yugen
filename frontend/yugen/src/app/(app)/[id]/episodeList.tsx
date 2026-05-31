"use client"

import * as api from "@lib/api.local"
import React, { Dispatch, SetStateAction, useEffect, useState } from "react";

import "./episodeList.css"
import { MediaEpisodeInfo, MediaInfo } from "@/app/shared/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";

interface Props {
    mediaInfo: MediaInfo,
    setSelectedItem: (info: MediaEpisodeInfo | undefined) => void,
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
    const searchParams = useSearchParams();

    const [episodes, setEpisodes] = useState<MediaEpisodeInfo[]>([]);
    const [loadingEpisodes, setLoadingEpisodes] = useState(false);

    const [timeUntil, setTimeUntil] = useState("")
    const [selectedEpisodeIndex, setSelectedEpisodeIndex] = useState<number | null>(null);

    const [refreshMenuOpen, setRefreshMenuOpen] = useState(false);


    useEffect(() => {
        fetchEpisodes(false, false);

        if (props.mediaInfo.upcomingEpisode == null) return;

        const update = () => {
            const ms = props.mediaInfo.upcomingEpisode! * 1000 - Date.now();

            const seconds = Math.floor(ms / 1000) % 60;
            const minutes = Math.floor(ms / (1000 * 60)) % 60;
            const hours = Math.floor(ms / (1000 * 60 * 60)) % 24;
            const days = Math.floor(ms / (1000 * 60 * 60 * 24));

            setTimeUntil(`${days}d ${hours}h ${minutes}m ${seconds}s`);
        };

        update();

        const interval = setInterval(update, 1000);
        return () => clearInterval(interval);

    }, [props.mediaInfo])

    useEffect(() => {

        const urlBased: string | null = searchParams.get("episode");

        if (urlBased != null) {
            const selected = Math.min(Math.max(Number.parseInt(urlBased), 0), episodes.length - 1);
            onSelectEpisode(selected);
            return;
        }

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
        window.history.replaceState(null, "", pos != null ? `?episode=${pos}` : "");

        setSelectedEpisodeIndex(pos);
        props.setSelectedItem(pos == null ? undefined : episodes[pos]);
    }

    const fetchEpisodes = (recache: boolean, clearOld: boolean) => {
        setRefreshMenuOpen(false);
        setLoadingEpisodes(true);

        api.library_GetEpisodes(props.mediaInfo.id, recache, clearOld)
            .then(r => setEpisodes(r.sort((a, b) => a.number - b.number)))
            .finally(() => setLoadingEpisodes(false));
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
                                    <>Unknown</>
                                )
                        }
                    </div>
                ) : (<>
                    <div className="EpisodeList_Titlebar">
                        <h2>Episodes</h2>
                        <div>
                            <button onClick={() => fetchEpisodes(false, false)}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                    <path fillRule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2z" />
                                    <path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466" />
                                </svg>
                            </button>
                            <button onClick={() => setRefreshMenuOpen(true)}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M3 9.5a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3m5 0a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3m5 0a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3" />
                                </svg>
                            </button>

                            {
                                refreshMenuOpen && (<div className="EpisodeList_Titlebar_RefreshMenu">
                                    <button onClick={() => fetchEpisodes(true, false)}>Recache</button>
                                    <button onClick={() => fetchEpisodes(true, true)}>Clear And Refetch</button>
                                </div>)
                            }
                        </div>
                    </div>
                    <div className="EpisodeList_Entries">
                        {
                            loadingEpisodes ? (
                                <>
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                    <div className="Episode_Skeleton" />
                                </>
                            ) : (
                                episodes?.map(drawEpisode)
                            )
                        }
                    </div>
                    {
                        props.mediaInfo.upcomingEpisode != null && (
                            <div className="EpisodeList_Upcoming">
                                {timeUntil}
                            </div>
                        )
                    }
                </>)}
        </div>
    )
}