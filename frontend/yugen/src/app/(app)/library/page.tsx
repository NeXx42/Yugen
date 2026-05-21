"use client"

import * as api from "@lib/api.local"

import { MediaCardInfo, PageResponse } from "@shared/types"
import { ReactNode, useEffect, useState } from "react"

import MediaCard from "@comps/mediaCard"
import PageContainer from "@comps/pageContainer"

import "./page.css"

type LibraryGroup = "Downloaded" | "Watching" | "OnHold" | "Planning" | "Completed" | "Dropped";

export default function () {
    const pageSize = 56;

    const [selectedGroup, setSelectedGroup] = useState<LibraryGroup>("Downloaded");
    const [currentPage, setCurrentPage] = useState(1);

    const search = (): Promise<PageResponse<MediaCardInfo>> => api.library_Search(currentPage - 1, pageSize, selectedGroup);

    const drawGroupBtn = (group: LibraryGroup, label: string | undefined = undefined): ReactNode => {
        const callback = () => {
            setCurrentPage(1);
            setSelectedGroup(group);
        }

        return (<button className={group === selectedGroup ? "Selected" : ""} onClick={callback}>{label ?? group}</button>)
    }

    const drawCard = (inp: MediaCardInfo): ReactNode => {
        return <MediaCard key={inp.aniListId} Card={inp} />
    }

    return (
        <div className="Library">
            <div className="Library_Filters">
                {drawGroupBtn("Downloaded")}
                {drawGroupBtn("Watching")}
                {drawGroupBtn("OnHold", "On-Hold")}
                {drawGroupBtn("Planning")}
                {drawGroupBtn("Completed")}
                {drawGroupBtn("Dropped")}
            </div>

            <div className="Library_Items">
                {<PageContainer search={search} currentPage={currentPage} setCurrentPage={setCurrentPage} pageSize={pageSize} drawElement={drawCard} track={[selectedGroup]} />}
            </div>
        </div >
    )
}