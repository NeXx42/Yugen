"use client"

import { useEffect, useState } from "react"

import * as api from "@lib/api.local"

import "./mediaPlayer.css"

export default function ({ ItemId }: { ItemId: string }) {
    const [playbackUrl, setPlaybackUrl] = useState<string | undefined>()

    const startPlayback = () => {
        setPlaybackUrl("http://localhost:5138/api/media/play");
    }

    return (
        <div className="MediaPlayer" onClick={startPlayback}>
            {playbackUrl != undefined &&
                <iframe
                    src="https://jellyfin.local/web/index.html#!/details?id=3a1340d44e2b8c59eb25226608786fb6"
                    allow="autoplay; fullscreen"
                ></iframe>
            }
        </div>
    )
}