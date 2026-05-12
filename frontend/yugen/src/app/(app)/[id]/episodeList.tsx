"use client"

import { MediaEpisodeInfo } from "@shared/types";
import React, { Dispatch, SetStateAction, useState } from "react";

import "./episodeList.css"

export default function ({ episodes, selectedItem, setSelectedItem }: { episodes: MediaEpisodeInfo[], selectedItem: string | undefined, setSelectedItem: Dispatch<SetStateAction<string | undefined>> }) {

    const drawEpisode = (ep: MediaEpisodeInfo): React.ReactNode => {
        return (
            <div className="Episode" key={ep.number}>
                <p>{ep.title ?? `Episode ${ep.number}`}</p>
            </div>
        )
    }

    return (
        <div className="EpisodeList">
            {episodes.map(drawEpisode)}
        </div>
    )
}