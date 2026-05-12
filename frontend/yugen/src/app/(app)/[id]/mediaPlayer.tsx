"use client"

import { useEffect, useState } from "react"

import * as api from "@lib/api.local"

import "./mediaPlayer.css"

export default function ({ itemId }: { itemId: string | undefined }) {
    const [playbackUrl, setPlaybackUrl] = useState<string | undefined>()

    const startPlayback = () => {
        setPlaybackUrl("http://localhost:5138/api/media/play");
    }

    return (
        <div className="MediaPlayer" onClick={startPlayback}>
            {playbackUrl != undefined && itemId != undefined &&
                <iframe
                    src={`https://jellyfin.local/web/index.html#!/details?id=${itemId}`}
                    allow="autoplay; fullscreen"
                ></iframe>
            }
        </div>
    )
}