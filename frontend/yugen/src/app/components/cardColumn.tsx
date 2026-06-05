"use client"

import { ReactNode, useEffect, useState } from "react"

import { MediaCardInfo } from "../shared/types"
import MediaCardHorizontal from "./mediaCardHorizontal";

import "./cardColumn.css"

interface Props {
    header: string;
    limit?: number;
    content: MediaCardInfo[];

    drawer?: (card: MediaCardInfo) => ReactNode
}

export default function (props: Props) {
    const [loadedContent, setLoadedContent] = useState<MediaCardInfo[] | undefined>(undefined);
    const [loadedAll, setLoadedAll] = useState(false);

    useEffect(() => {
        setLoadedContent(props.content);
    }, [props.content]);

    const drawElement = (card: MediaCardInfo) => {
        if (props.drawer != null)
            return props.drawer(card);

        return <MediaCardHorizontal key={card.aniListId} card={card} season={undefined} />;
    }

    return (
        <div className="CardColumn">
            <h2>{props.header}</h2>
            {loadedContent?.slice(0, loadedAll ? (loadedContent?.length ?? 0) : (props.limit ?? 10)).map(drawElement)}
            {!loadedAll && (props.limit ?? 10) < (loadedContent?.length ?? 0) && <button onClick={() => setLoadedAll(true)} >
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 320 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                    <path d="M143 352.3L7 216.3c-9.4-9.4-9.4-24.6 0-33.9l22.6-22.6c9.4-9.4 24.6-9.4 33.9 0l96.4 96.4 96.4-96.4c9.4-9.4 24.6-9.4 33.9 0l22.6 22.6c9.4 9.4 9.4 24.6 0 33.9l-136 136c-9.2 9.4-24.4 9.4-33.8 0z" />
                </svg>
            </button>}
        </div>
    )
}