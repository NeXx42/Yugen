"use client"

import * as api from "@lib/api.local"
import { useEffect, useState } from "react";
import { MediaCardInfo, PageResponse } from "@shared/types";
import CardRow from "@/app/components/cardRow";

export default function () {
    const [isLoading, setLoading] = useState(false)
    const [cards, setCards] = useState<MediaCardInfo[]>([])

    useEffect(() => {
        setLoading(true);
        api.library_CurrentWatching(0, 10).then(r => setCards(r.data.sort((a, b) => (b.watchLastTime ?? 0) - (a.watchLastTime ?? 0)))).finally(() => setLoading(false));
    }, [fetch])

    return (
        <div>
            <h1 style={{ marginBottom: "15px" }}>Continue Watching</h1>
            <CardRow cards={cards} />
        </div>
    )
}