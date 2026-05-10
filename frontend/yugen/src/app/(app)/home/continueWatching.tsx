"use client"

import { MediaCard } from "@shared/types";
import * as api from "@lib/api.local"
import { useEffect, useState } from "react";

export default function () {
    const [isLoading, setLoading] = useState(false)
    const [cards, setCards] = useState<MediaCard[] | undefined>(undefined)

    useEffect(() => {
        setLoading(true);
        api.library_CurrentWatching().then(setCards).finally(() => setLoading(false));
    }, [])

    return (
        <div>
            <h1>Continue Watching</h1>
            <div>
                <ol>

                    {cards?.map((x, i) => (<li><a key={`${x.title}_${i}`}>{x.title}</a></li>))}
                </ol>
            </div>
        </div>
    )
}