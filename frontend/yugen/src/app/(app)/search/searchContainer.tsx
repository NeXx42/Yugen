"use client"

import { useEffect, useState } from "react"

import * as api from "@lib/api.local"
import { MediaCardInfo, PageResponse, SearchRequest } from "@shared/types"
import MediaCard from "@/app/components/mediaCard";


import "./searchContainer.css"
import PageContainer from "@/app/components/pageContainer";

export default function ({ searchQuery }: { searchQuery: SearchRequest }) {
    const pageSize = 54;

    const [currentPage, setCurrentPage] = useState(1);
    const search = (): Promise<PageResponse<MediaCardInfo>> => api.catalog_Search({
        ...searchQuery,
        page: currentPage,
        pageSize,
    });

    useEffect(() => updateUrl(), [searchQuery, currentPage])

    const updateUrl = () => {
        const val: string[] = [];
        val.push(`&page=${currentPage}`);

        if (searchQuery.text) val.push(`query=${searchQuery.text}`);
        if (searchQuery.format) val.push(`format=${searchQuery.format}`);
        if (searchQuery.status) val.push(`status=${searchQuery.status}`);
        if (searchQuery.year) val.push(`year=${searchQuery.year}`);

        if ((searchQuery.genres?.length ?? 0) > 0) val.push(`genres=${searchQuery.genres!.join(",")}`);
        if ((searchQuery.tags?.length ?? 0) > 0) val.push(`tags=${searchQuery.tags!.join(",")}`);

        window.history.replaceState(null, "", `?${val.join("&")}`);
    }

    return (
        <div className="SearchContainer">
            <PageContainer search={search} pageSize={pageSize} currentPage={currentPage} setCurrentPage={setCurrentPage} drawElement={e => <MediaCard key={e.aniListId} Card={e} />} track={[searchQuery]} />
        </div>
    )
}