"use client"

import { Dispatch, ReactNode, SetStateAction, useEffect, useState } from "react";
import { PageResponse } from "../shared/types";

import "./pageContainer.css"

interface Props<T> {
    pageSize: number,
    currentPage: number,
    setCurrentPage: Dispatch<SetStateAction<number>>,

    search: () => Promise<PageResponse<T>>
    drawElement: (inp: T) => ReactNode;

    track: any[] | undefined,
}

export default function <T>(props: Props<T>) {
    const [isLoading, setLoading] = useState(false);
    const [error, setError] = useState<any | undefined>(undefined);
    const [content, SetContent] = useState<PageResponse<T> | undefined>();

    useEffect(() => {
        setLoading(true);
        setError(undefined)

        props.search().then(SetContent).catch(setError).finally(() => setLoading(false));

    }, [...(props.track ?? []), props.currentPage, props.pageSize])

    const getVisiblePages = (currentPage: number, totalPages: number, maxVisible: number = 4): number[] => {
        const half = Math.floor(maxVisible / 2)

        let start = currentPage - half
        let end = currentPage + half

        if (maxVisible % 2 === 0) {
            end -= 1 // keeps even window balanced
        }

        if (start < 0) {
            start = 0
            end = maxVisible - 1
        }

        if (end >= totalPages) {
            end = totalPages - 1
            start = Math.max(0, end - maxVisible + 1)
        }

        const pages: number[] = []
        for (let i = start; i <= end; i++) {
            pages.push(i)
        }

        return pages
    }

    const drawPageControls = (): ReactNode => {
        const totalPages = Math.ceil((content?.totalResults ?? 0) / props.pageSize)
        const pages = getVisiblePages(props.currentPage, totalPages, 4)

        return (
            <div style={{ display: "flex", gap: 6, alignItems: "center" }}>
                <button onClick={() => props.setCurrentPage(0)} disabled={props.currentPage === 0}>{"<<"}</button>
                <button onClick={() => props.setCurrentPage((p) => Math.max(0, p - 1))} disabled={props.currentPage === 0}>{"<"}</button>

                {pages.map((p) => (
                    <button key={p} onClick={() => props.setCurrentPage(p)} className={p == props.currentPage ? "Selected" : ""}>{p + 1} </button>
                ))}

                <button onClick={() => props.setCurrentPage((p) => Math.min(totalPages - 1, p + 1))} disabled={props.currentPage === totalPages - 1}> {">"}</button>
                <button onClick={() => props.setCurrentPage(totalPages - 1)} disabled={props.currentPage === totalPages - 1} > {">>"} </button>
            </div>
        );
    }

    const drawContent = (): ReactNode => {
        if (error) {
            return (<a>{error.message}</a>)
        }

        if (isLoading)
            return (<a>Loading</a>)

        return content?.data.map(props.drawElement);
    }

    return (<div className="Paginator">
        <div className="Page_Top">
            <a>Results {content?.totalResults ?? 0}</a>
        </div>
        <div className="Page_Content">
            {drawContent()}
        </div>
        <div className="Page_Pages">
            {drawPageControls()}
        </div>
    </div>)
}