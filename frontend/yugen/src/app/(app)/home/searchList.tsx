"use client"

import * as api from "@lib/api.local"

import "./searchList.css"
import { useEffect, useState } from "react"
import { MediaCardInfo, PageResponse } from "@/app/shared/types"
import MediaCard from "@/app/components/mediaCard"

export default function () {
    const [cards, setCards] = useState<PageResponse<MediaCardInfo>>()
    const [loading, setLoading] = useState(false);

    const [sort, setSort] = useState<number>(13)
    const [page, setPage] = useState<number>(1)

    useEffect(() => {
        setLoading(true);
        api.catalog_Search({
            page: page,
            pageSize: 35,

            text: null,
            sort: sort
        }).then(setCards).finally(() => setLoading(false));

    }, [sort, page])

    const updateSort = (to: number) => {
        setPage(1);
        setSort(to);
    }

    return (
        <div className="SearchList">
            <div className="SearchList_Controls">
                <div className="SearchList_Sort">
                    <button className={sort === 13 ? "Selected" : ""} onClick={() => updateSort(13)}>Newest</button>
                    <button className={sort === 19 ? "Selected" : ""} onClick={() => updateSort(19)}>Popular</button>
                    <button className={sort === 17 ? "Selected" : ""} onClick={() => updateSort(17)}>Top Rated</button>
                </div>
                <div className="SearchList_Page">
                    <button disabled={page >= 1} onClick={() => setPage(page - 1)}>
                        <svg stroke="currentColor" fill="currentColor" viewBox="0 0 320 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                            <path d="M41.4 233.4c-12.5 12.5-12.5 32.8 0 45.3l160 160c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L109.3 256 246.6 118.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0l-160 160z" />
                        </svg>
                    </button>
                    <a>{page}</a>
                    <button onClick={() => setPage(page + 1)}>
                        <svg stroke="currentColor" fill="currentColor" viewBox="0 0 320 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                            <path d="M278.6 233.4c12.5 12.5 12.5 32.8 0 45.3l-160 160c-12.5 12.5-32.8 12.5-45.3 0s-12.5-32.8 0-45.3L210.7 256 73.4 118.6c-12.5-12.5-12.5-32.8 0-45.3s32.8-12.5 45.3 0l160 160z" />
                        </svg>
                    </button>
                </div>
            </div>

            <div className="SearchList_Content">
                {
                    loading ? (
                        <a>Loading</a>
                    ) : (

                        cards?.data.map(c => <MediaCard key={c.aniListId} Card={c} />)
                    )}
            </div>
        </div >
    )
}