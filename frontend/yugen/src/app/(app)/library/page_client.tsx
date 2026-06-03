"use client"

import * as api from "@lib/api.local"

import { MediaCardInfo, PageResponse, SearchCriteria } from "@shared/types"
import { ReactNode, useEffect, useState } from "react"

import MediaCard from "@comps/mediaCard"
import PageContainer from "@comps/pageContainer"

import "./page.css"
import { useSearchParams } from "next/navigation"
import GenericSearchContainer from "@/app/components/genericSearchContainer"

type LibraryGroup = "ContinueWatching" | "Downloaded" | "Watching" | "OnHold" | "Planning" | "Completed" | "Dropped";

export default function ({ criteria }: { criteria: SearchCriteria | null }) {
    const searchParams = useSearchParams();
    const pageSize = 56;

    const [refresh, setRefresh] = useState<number>(0);

    const [selectedGroup, setSelectedGroup] = useState<LibraryGroup>(() => {
        const group = searchParams.get("group");

        const validGroups: LibraryGroup[] = [
            "ContinueWatching",
            "Downloaded",
            "Watching",
            "OnHold",
            "Planning",
            "Completed",
            "Dropped",
        ];

        if (group && validGroups.includes(group as LibraryGroup)) {
            return group as LibraryGroup;
        }

        return "ContinueWatching";
    });

    const [currentPage, setCurrentPage] = useState<number>(() => {
        const page = searchParams.get("page");

        if (page) {
            const parsed = Number.parseInt(page, 10);

            if (!Number.isNaN(parsed) && parsed > 0) {
                return parsed;
            }
        }

        return 1;
    });

    useEffect(() => {
        window.history.replaceState(null, "", selectedGroup != null ? `?group=${selectedGroup}&page=${currentPage}` : "");
    }, [selectedGroup, currentPage])

    const search = (): Promise<PageResponse<MediaCardInfo>> => api.library_Search(currentPage - 1, pageSize, selectedGroup);

    const drawGroupBtn = (group: LibraryGroup, label: string | undefined = undefined): ReactNode => {
        const callback = () => {
            setCurrentPage(1);
            setSelectedGroup(group);
        }

        return (<button className={group === selectedGroup ? "Selected" : ""} onClick={callback}>{label ?? group}</button>)
    }

    const drawCard = (inp: MediaCardInfo): ReactNode => {
        return <MediaCard key={inp.aniListId} Card={inp} requestRefresh={() => setRefresh(refresh + 1)} />
    }
    return (
        <div className="Library">
            <GenericSearchContainer criteria={criteria} />

            <div className="Library_Filters">
                {drawGroupBtn("ContinueWatching", "Continue Watching")}
                {drawGroupBtn("Downloaded")}
                {drawGroupBtn("Watching")}
                {drawGroupBtn("OnHold", "On-Hold")}
                {drawGroupBtn("Planning")}
                {drawGroupBtn("Completed")}
                {drawGroupBtn("Dropped")}
            </div>

            <div className="Library_Items">
                {<PageContainer search={search} currentPage={currentPage} setCurrentPage={setCurrentPage} pageSize={pageSize} drawElement={drawCard} track={[selectedGroup, refresh]} />}
            </div>
        </div >
    )
}