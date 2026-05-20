"use client"

import { MediaCardInfo } from "@shared/types";
import { useRouter } from "next/navigation";

import "./mediaCardHorizontal.css"

export default function ({ Card }: { Card: MediaCardInfo }) {
    const navigate = useRouter();

    const navigateToPage = () => {
        navigate.push(`${Card.aniListId}`)
    };


    return (<div key={Card.aniListId} className="MediaCardHorizontal" onClick={navigateToPage}>
        <div className="MediaCardHorizontal_Content">
            <img src={Card.cardImg} />

            <div className="MediaCardHorizontal_Info">
                <h2>{Card.title}</h2>
                <a>{Card.type}</a>
            </div>
        </div>

        {Card.bannerImage && <img src={Card.bannerImage} />}
    </div>)
}