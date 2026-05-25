"use client"

import { ReactNode, useEffect, useState } from "react"

import { MediaCardInfo } from "../shared/types"
import MediaCardHorizontal from "./mediaCardHorizontal";

import "./cardColumn.css"

interface Props {
    header: string;
    limit?: number;
    content?: MediaCardInfo[];
    loader?: (() => Promise<MediaCardInfo[]>);

    drawer?: (card: MediaCardInfo) => ReactNode
}

export default function (props: Props) {
    const [loadedContent, setLoadedContent] = useState<MediaCardInfo[] | undefined>(undefined);
    const [loadedAll, setLoadedAll] = useState(false);

    useEffect(() => {
        if (props.loader != undefined) {
            props.loader()!.then(setLoadedContent);
        }
        else {
            setLoadedContent(props.content)
        }

        setLoadedAll(false);
    }, [props.content, props.loader]);

    const drawElement = (card: MediaCardInfo) => {
        if (props.drawer != null)
            return props.drawer(card);

        return <MediaCardHorizontal key={card.aniListId} card={card} season={undefined} />;
    }

    return (
        <div className="CardColumn">
            <h2>{props.header}</h2>
            {loadedContent?.slice(0, loadedAll ? (loadedContent?.length ?? 0) : (props.limit ?? 10)).map(drawElement)}
            {!loadedAll && (props.limit ?? 10) < (loadedContent?.length ?? 0) && <button onClick={() => setLoadedAll(true)} >Show Rest</button>}
        </div>
    )
}