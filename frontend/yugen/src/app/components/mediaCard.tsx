"use client"

import { MediaCardInfo } from "@shared/types";
import { useRouter } from "next/navigation";

import "./mediaCard.css"

export default function ({ Card }: { Card: MediaCardInfo }) {
    const navigate = useRouter();

    const navigateToPage = () => {
        navigate.push(`${Card.id}`)
    };

    return (<div className="MediCard" onClick={navigateToPage}>
        <h2>{Card.title}</h2>
    </div>)
}