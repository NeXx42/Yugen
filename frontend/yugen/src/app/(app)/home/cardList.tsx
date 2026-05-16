"use client"
import { MediaCardInfo } from "@shared/types";

import { useEffect, useState } from "react";
import MediaCard from "@comps/mediaCard";

import "./cardList.css"
import CardRow from "@/app/components/cardRow";

export default function ({ fetch }: { fetch: Promise<MediaCardInfo[]> }) {
    const [isLoading, setLoading] = useState(false)
    const [cards, setCards] = useState<MediaCardInfo[] | undefined>(undefined)

    useEffect(() => {
        setLoading(true);
        fetch.then(setCards).finally(() => setLoading(false));
    }, [fetch])

    return (
        <div>
            <h1 style={{ marginBottom: "15px" }}>Continue Watching</h1>
            <CardRow cards={cards ?? []} />
        </div>
    )
}