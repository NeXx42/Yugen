"use client"

import * as api from "@lib/api.local"
import { MediaCardInfo } from "@shared/types";

import { useEffect, useState } from "react";
import MediaCard from "@comps/mediaCard";

export default function () {
    const [isLoading, setLoading] = useState(false)
    const [cards, setCards] = useState<MediaCardInfo[] | undefined>(undefined)

    useEffect(() => {
        setLoading(true);
        api.library_CurrentWatching().then(setCards).finally(() => setLoading(false));
    }, [])

    return (
        <div>
            <h1>Continue Watching</h1>
            <div>
                {cards?.map(x => <MediaCard Card={x} key={x.id} />)}
            </div>
        </div>
    )
}