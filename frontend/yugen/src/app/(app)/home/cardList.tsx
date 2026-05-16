"use client"
import { MediaCardInfo } from "@shared/types";

import { useEffect, useState } from "react";
import MediaCard from "@comps/mediaCard";

import "./cardList.css"

export default function ({ fetch }: { fetch: Promise<MediaCardInfo[]> }) {
    const [isLoading, setLoading] = useState(false)
    const [cards, setCards] = useState<MediaCardInfo[] | undefined>(undefined)

    useEffect(() => {
        setLoading(true);
        fetch.then(setCards).finally(() => setLoading(false));
    }, [fetch])

    return (
        <div>
            <h1>Continue Watching</h1>
            <div className="CardList_Cards">
                {cards?.slice(0, 10).map(x => <MediaCard Card={x} key={x.aniListId} />)}
            </div>
        </div>
    )
}