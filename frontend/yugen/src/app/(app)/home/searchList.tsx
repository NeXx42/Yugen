"use client"

import * as api from "@lib/api.local"

import "./searchList.css"
import { useEffect, useState } from "react"
import { MediaCardInfo } from "@/app/shared/types"
import MediaCard from "@/app/components/mediaCard"

export default function () {
    const [cards, setCards] = useState<MediaCardInfo[]>()
    const [sort, setSort] = useState<number>(33)

    useEffect(() => {

        api.catalog_Search({
            page: 0,
            pageSize: 20,

            text: null,
            sort: sort
        }).then(r => setCards(r.data));

    }, [sort])

    return (
        <div className="SearchList">
            <div className="SearchList_Sort">
                <button className={sort === 33 ? "Selected" : ""} onClick={() => setSort(33)}>Newest</button>
                <button className={sort === 19 ? "Selected" : ""} onClick={() => setSort(19)}>Popular</button>
                <button className={sort === 17 ? "Selected" : ""} onClick={() => setSort(17)}>Top Rated</button>
            </div>
            <div className="SearchList_Content">
                {cards?.map(c => <MediaCard key={c.aniListId} Card={c} />)}
            </div>
        </div >
    )
}