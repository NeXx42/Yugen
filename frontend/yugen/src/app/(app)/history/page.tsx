"use client"

import * as api from "@lib/api.local"

import PageContainer from "@/app/components/pageContainer";
import { MediaCardInfo, PageResponse } from "@/app/shared/types";
import { useState } from "react";
import MediaCard from "@/app/components/mediaCard";

import "./page.css"

export default function () {
    const pageSize = 54;
    const [currentPage, setCurrentPage] = useState(1)

    const search = (): Promise<PageResponse<MediaCardInfo>> => api.library_CurrentWatching(0, pageSize);

    return (
        <div className="History">
            <PageContainer search={search} currentPage={currentPage} setCurrentPage={setCurrentPage} drawElement={e => <MediaCard key={e.aniListId} Card={e} />} pageSize={pageSize} track={[]} />
        </div>
    )
}