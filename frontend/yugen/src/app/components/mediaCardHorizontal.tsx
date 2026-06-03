"use client"

import { MediaCardInfo } from "@shared/types";
import { useRouter } from "next/navigation";

import "./mediaCardHorizontal.css"

interface Props {
    card: MediaCardInfo,
    season?: number
    selected?: boolean
}

export default function (props: Props) {
    const navigate = useRouter();

    const navigateToPage = () => {
        navigate.push(`${props.card.aniListId}`)
    };

    const getTimeTo = () => {
        const diff = (props.card.nextReleaseDate! * 1000) - Date.now();

        if (diff <= 0) return "now";

        const minutes = Math.floor(diff / (1000 * 60));
        const hours = Math.floor(diff / (1000 * 60 * 60));
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));

        if (days > 0) return `${days}d`;
        if (hours > 0) return `${hours}h`;
        return `${minutes}m`
    }

    return (<a key={props.card.aniListId} className={`MediaCardHorizontal ${props.selected ? "Selected" : ""}`} href={`${props.card.aniListId}`}>
        <div className="MediaCardHorizontal_Content">
            {props.card.cardImg && <img className="MediaCardHorizontal_Icon" src={props.card.cardImg} />}
            {props.card.banner &&
                <div className="MediaCardHorizontal_Banner">
                    <img src={props.card.banner} />
                    <div />
                </div>
            }

            {props.card.nextReleaseDate != undefined && <div className="MediaCardHorizontal_Status" />}
            {props.card.nextReleaseDate != undefined && <div className="MediaCardHorizontal_ReleaseDate" >{getTimeTo()}</div>}

            <div className="MediaCardHorizontal_Info">
                <h2>{props.season ? `Season ${props.season}` : props.card.title}</h2>
                <div>
                    {props.card.type != null && <p>{props.card.type}</p>}
                    {props.card.year != null && <p>{props.card.year}</p>}
                    {props.card.season != null && <p>{props.card.season}</p>}
                </div>
            </div>
        </div>
    </a>)
}