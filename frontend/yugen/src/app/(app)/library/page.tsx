"use client"

import * as api from "@lib/api.local"

import { MediaCardInfo } from "@/app/shared/types"
import { ReactNode, useEffect, useState } from "react"

import MediaCard from "@/app/components/mediaCard"

import "./page.css"

type LibraryGroup = "Downloaded" | "Watching" | "OnHold" | "Planning" | "Completed" | "Dropped";

export default function () {
    const [selectedGroup, setSelectedGroup] = useState<LibraryGroup>("Downloaded");
    const [page, setPage] = useState(0)

    const [isLoading, setLoading] = useState(false);
    const [content, SetContent] = useState<MediaCardInfo[] | undefined>();

    useEffect(() => {

        setLoading(true);
        api.library_Search(page, 54, selectedGroup).then(SetContent).finally(() => setLoading(false));

    }, [page, selectedGroup]);

    const drawGroupBtn = (group: LibraryGroup, label: string | undefined = undefined): ReactNode => {
        return (<button className={group === selectedGroup ? "Selected" : ""} onClick={() => setSelectedGroup(group)}>{label ?? group}</button>)
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
                {
                    isLoading ? (
                        <a>loading</a>
                    ) : (
                        content?.map(c => <MediaCard key={c.aniListId} Card={c} />)
                    )
                }
            </div>
        </div>
    )
}