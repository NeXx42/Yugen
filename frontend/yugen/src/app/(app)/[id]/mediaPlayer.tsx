import { ReactNode, useEffect, useState } from "react"

import "./mediaPlayer.css"
import { MediaEpisodeInfo, MediaInfo } from "@/app/shared/types"

interface Props {
    mediaInfo: MediaInfo
    episode: MediaEpisodeInfo | undefined
}

export default function (props: Props) {
    const thumbnail = props.episode?.thumbnail ?? props.mediaInfo.thumbnailImage;
    const [playingId, setPlayingId] = useState<string | undefined>(undefined)

    useEffect(() => setPlayingId(undefined), [props.mediaInfo, props.episode])

    const playMedia = (to: string | undefined) => {
        setPlayingId(to);
    }

    return (
        <div className="MediaPlayer">
            <div className="MediaPlayer_Container">
                {playingId != undefined ? (
                    <iframe
                        src={`https://jellyfin.local/web/index.html#!/details?id=${playingId!}`}
                        allow="autoplay; fullscreen"
                    ></iframe>
                ) :
                    (
                        <div className="MediaPlayer_Container_Request" onClick={() => playMedia(props.episode?.jellyfinId ?? undefined)}>
                            {thumbnail != undefined && <img src={thumbnail ?? ""} />}

                            <svg stroke="currentColor" fill="currentColor" viewBox="0 0 448 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                                <path d="M424.4 214.7L72.4 6.6C43.8-10.3 0 6.1 0 47.9V464c0 37.5 40.7 60.1 72.4 41.3l352-208c31.4-18.5 31.5-64.1 0-82.6z"></path>
                            </svg>
                        </div>
                    )
                }
            </div>
            <div className="MediaPlayer_Controls ViewPageContainer">
                <div className="MediaPlayer_Controls_Bookmark">

                </div>
            </div>
        </div>
    )
}