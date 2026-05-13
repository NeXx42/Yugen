"use client"

import * as api from "@lib/api.local"

import { MediaEpisodeInfo, SonarrEpisodeInfo } from "@shared/types";
import React, { Dispatch, SetStateAction, useEffect, useState } from "react";

import "./episodeList.css"

interface Props {
    mediaId: number,
    episodes: MediaEpisodeInfo[],

    selectedItem: string | undefined,
    setSelectedItem: Dispatch<SetStateAction<string | undefined>>,
}

export default function (props: Props) {

    const [selectedEpisode, setSelectedEpisode] = useState(0);

    const [loading, setLoading] = useState(false);
    const [sonarrEpisodeInfo, setSonarrEpisodeInfo] = useState<SonarrEpisodeInfo[] | undefined>();

    useEffect(() => {

        setLoading(true);
        api.library_GetSonarrEpisodes(props.mediaId).then(setSonarrEpisodeInfo).finally(() => setLoading(false));

    }, [props.episodes])

    const drawEpisode = (ep: MediaEpisodeInfo, pos: number): React.ReactNode => {
        return (
            <div className={`Episode ${pos == selectedEpisode ? "Episode-Selected" : ""}`} key={ep.number} onClick={() => setSelectedEpisode(pos)}>
                <p>{ep.title ?? `Episode ${ep.number}`}</p>
            </div>
        )
    }

    return (
        <div className="EpisodeList">
            {props.episodes.map(drawEpisode)}
        </div>
    )
}