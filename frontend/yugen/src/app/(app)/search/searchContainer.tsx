"use client"

import { useEffect, useState } from "react"

import * as api from "@lib/api.local"
import { MediaCardInfo, PageResponse } from "@shared/types"
import MediaCard from "@/app/components/mediaCard";


import "./searchContainer.css"
import PageContainer from "@/app/components/pageContainer";

export default function ({ searchQuery }: { searchQuery: string }) {
    const pageSize = 54;

    const [currentPage, setCurrentPage] = useState(0);
    const search = (): Promise<PageResponse<MediaCardInfo>> => api.catalog_Search({
        page: currentPage,
        pageSize,

        text: searchQuery,
    });

    return (
        <div className="SearchContainer">
            <PageContainer search={search} pageSize={pageSize} currentPage={currentPage} setCurrentPage={setCurrentPage} drawElement={e => <MediaCard key={e.aniListId} Card={e} />} track={[searchQuery]} />
        </div>
    )
}