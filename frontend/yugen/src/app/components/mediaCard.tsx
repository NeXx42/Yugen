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

    return (<div className="MediCard" onClick={navigateToPage}>
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
            </div>
            <div className="MediaCard_Content">
                <h3>{Card.title}</h3>
            </div>
        </div>
    </div>)
}