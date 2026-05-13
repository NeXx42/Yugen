"use client"

import { MediaInfo } from "@/app/shared/types";
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";
import { useState } from "react";

import "./mediaContainer.css"

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const [selectedItem, setSelectedItem] = useState<string | undefined>(undefined)

    return (
        <div className="MediaContainer">
            <div className="MediaContainer_Items">
                <MediaPlayer itemId={selectedItem} />
                <EpisodeList mediaId={mediaInfo.id} selectedItem={selectedItem} setSelectedItem={setSelectedItem} episodes={mediaInfo.episodes} />
            </div>

            <div className="MediaContainer_Seasons">

            </div>
        </div>
    )
}