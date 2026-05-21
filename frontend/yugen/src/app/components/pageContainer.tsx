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
            <button disabled={props.currentPage <= 1} onClick={() => props.setCurrentPage(props.currentPage - 1)}>
                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 320 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M41.4 233.4c-12.5 12.5-12.5 32.8 0 45.3l160 160c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L109.3 256 246.6 118.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0l-160 160z" />
                </svg>
            </button>
            <a>{props.currentPage}</a>
            <button disabled={(props.currentPage * (content?.pageSize ?? 0)) > (content?.totalResults ?? 0)} onClick={() => props.setCurrentPage(props.currentPage + 1)}>
                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 320 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M278.6 233.4c12.5 12.5 12.5 32.8 0 45.3l-160 160c-12.5 12.5-32.8 12.5-45.3 0s-12.5-32.8 0-45.3L210.7 256 73.4 118.6c-12.5-12.5-12.5-32.8 0-45.3s32.8-12.5 45.3 0l160 160z" />
                </svg>
            </button>
        </div>
        <div className="Page_Content">
            {drawContent()}
        </div>
    </div>)
}