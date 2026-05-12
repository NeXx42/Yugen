"use client"

import { MediaCardInfo } from "@shared/types";
import { useRouter } from "next/navigation";

import "./mediaCard.css"

export default function ({ Card }: { Card: MediaCardInfo }) {
    const navigate = useRouter();

    const navigateToPage = () => {
        navigate.push(`${Card.aniListId}`)
    };

    return (<div className="MediCard" onClick={navigateToPage}>
        <img src={Card.cardImg} />
        <a>{Card.title}</a>
    </div>)
}