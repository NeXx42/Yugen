"use client"

import * as api from "@lib/api.local"

import { useEffect, useState } from "react";
import { MediaCardInfo } from "@shared/types";
import CardRow from "@/app/components/cardRow";

export default function () {
    const [cards, setCards] = useState<MediaCardInfo[] | undefined>(undefined)

    useEffect(() => {
        api.catalog_Upcoming().then(setCards);
    }, [fetch])

    return (
        <div>
            <h1 style={{ marginBottom: "15px" }}>Upcoming</h1>
            {cards == undefined ?
                <>Loading...</> :
                <CardRow cards={cards} />
            }
        </div>
    )
}