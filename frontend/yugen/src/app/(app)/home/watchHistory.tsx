"use client"

import * as api from "@lib/api.local"

import CardRow from "@/app/components/cardRow";
import { useEffect, useMemo } from "react";
import { MediaCardInfo, PageResponse } from "@/app/shared/types";
import { useRequest } from "@/app/effects/useRequest";

export default function () {
    const { data, execute } = useRequest<PageResponse<MediaCardInfo>>(() => api.library_CurrentWatching(1, 10));

    useEffect(() => { execute(); }, [])

    const watchingEntries: MediaCardInfo[] = useMemo(() => {
        if ((data?.data?.length ?? 0) > 0)
            return data!.data!.sort((a, b) => (b.watchLastTime ?? 0) - (a.watchLastTime ?? 0));

        return []
    }, [data])

    return (
        watchingEntries.length > 0 && (<div>
            <h1 style={{ marginBottom: "10px", fontSize: "26px" }}>Watch History</h1>
            <CardRow cards={watchingEntries} viewMoreLink={"library"} />
        </div>)
    )
}