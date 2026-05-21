"use client"

import { MediaCardInfo } from "@shared/types";
import { useRouter } from "next/navigation";

import "./mediaCard.css"

export default function ({ Card }: { Card: MediaCardInfo }) {
    const navigate = useRouter();

    const navigateToPage = () => {
        navigate.push(`${Card.aniListId}`)
    };

    const getNextReleaseText = (): string => {
        const diff = Card.nextReleaseDate! * 1000; // convert seconds → ms

        if (diff <= 0) return "now";

        const minutes = Math.floor(diff / (1000 * 60));
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));

        if (days > 0) {
            return `${days} day${days !== 1 ? "s" : ""}`;
        }

        if (hours > 0) {
            return `${hours} hour${hours !== 1 ? "s" : ""}`;
        }

        return `${minutes} minute${minutes !== 1 ? "s" : ""}`;
    }

    return (<div key={Card.aniListId} className="MediCard" onClick={navigateToPage} style={{ "--hover-color": Card.colour } as React.CSSProperties}>
        <div className="MediaCard_Container">
            <div className="MediaCard_Img">
                <img src={Card.cardImg} />

                {Card.nextReleaseDate != undefined && <div className="MediaCard_NextRelease">{getNextReleaseText()}</div>}

                {Card.watchEpisode != undefined && <div className="MediaCard_WatchedEpisode">EP {Card.watchEpisode}</div>}
                {Card.watchPercentage != undefined &&
                    <div className="MediCard_WatchPercentage">
                        <div style={{
                            width: `${Card.watchPercentage * 100}%`
                        }} />
                    </div>
                }

                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 448 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M424.4 214.7L72.4 6.6C43.8-10.3 0 6.1 0 47.9V464c0 37.5 40.7 60.1 72.4 41.3l352-208c31.4-18.5 31.5-64.1 0-82.6z"></path>
                </svg>
            </div>
            <div className="MediaCard_Content">
                <h3 >{Card.title}</h3>
                <div className="MediaCard_Content_Items">

                    {Card.type != undefined && <a>{Card.type}</a>}
                </div>
            </div>
        </div>
    </div>)
}