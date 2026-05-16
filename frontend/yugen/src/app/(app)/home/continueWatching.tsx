"use client"

import * as api from "@lib/api.local"
import { useEffect, useState } from "react";
import { MediaCardInfo } from "@shared/types";
import CardRow from "@/app/components/cardRow";

export default function () {
    const [isLoading, setLoading] = useState(false)
    const [cards, setCards] = useState<MediaCardInfo[] | undefined>(undefined)

    useEffect(() => {
        setLoading(true);
        api.library_CurrentWatching().then(setCards).finally(() => setLoading(false));
    }, [fetch])

    return (
        <div>
            <h1 style={{ marginBottom: "15px" }}>Continue Watching</h1>
            <CardRow cards={cards ?? []} />
        </div>
    )
}