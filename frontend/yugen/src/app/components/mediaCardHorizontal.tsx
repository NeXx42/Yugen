"use client"

import { MediaCardInfo } from "@shared/types";
import { useRouter } from "next/navigation";

import "./mediaCardHorizontal.css"

interface Props {
    card: MediaCardInfo,
    season: number | undefined
}

export default function ({ card, season = undefined }: Props) {
    const navigate = useRouter();

    const navigateToPage = () => {
        navigate.push(`${card.aniListId}`)
    };


    return (<div key={card.aniListId} className="MediaCardHorizontal" onClick={navigateToPage}>
        <div className="MediaCardHorizontal_Content">
            {card.cardImg && <img src={card.cardImg} />}

            <div className="MediaCardHorizontal_Info">
                <h2>{season ? `Season ${season}` : card.title}</h2>
                <a>{card.type}</a>
            </div>
        </div>

        {card.bannerImage && <img src={card.bannerImage} />}
    </div>)
}