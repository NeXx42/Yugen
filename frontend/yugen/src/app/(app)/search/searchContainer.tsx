"use client"

import { useEffect, useState } from "react"

import * as api from "@lib/api.local"
import { MediaCardInfo } from "@shared/types"
import MediaCard from "@/app/components/mediaCard";


import "./searchContainer.css"

export default function ({ searchQuery }: { searchQuery: string }) {
    const [loading, setLoading] = useState<boolean>(false);
    const [results, setResults] = useState<MediaCardInfo[] | undefined>(undefined);

    useEffect(() => {

        setLoading(true);
        api.catalog_Search(searchQuery).then(setResults).finally(() => setLoading(false));

    }, [searchQuery])

    if (loading)
        return (<>loading...</>)

    return (
        <div className="SearchContainer">
            {results?.map(e => <MediaCard Card={e} />)}
        </div>
    )
}