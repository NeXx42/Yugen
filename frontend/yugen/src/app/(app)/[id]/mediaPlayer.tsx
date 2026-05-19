import { ReactNode, useEffect, useState } from "react"

import "./mediaPlayer.css"
import { MediaEpisodeInfo, MediaInfo } from "@/app/shared/types"

interface Props {
    mediaInfo: MediaInfo
    episode: MediaEpisodeInfo | undefined
    bookmarkNode: ReactNode
}

export default function (props: Props) {
    const thumbnail = props.episode?.thumbnail ?? props.mediaInfo.thumbnailImage;
    const [playingId, setPlayingId] = useState<string | undefined>(undefined)

    useEffect(() => setPlayingId(undefined), [props.mediaInfo, props.episode])

    const playMedia = (to: string | undefined) => {
        setPlayingId(to);
    }

    const requestMedia = () => {

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
                        <div className="MediaPlayer_Container_Request">
                            {thumbnail != undefined && <img src={thumbnail ?? ""} />}
                            {props.episode?.jellyfinId != null ? (<button onClick={() => playMedia(props.episode!.jellyfinId!)}>play</button>) : (<button>Request</button>)}
                        </div>
                    )
                }
            </div>
            <div className="MediaPlayer_Controls">
                <div className="MediaPlayer_Controls_Bookmark">
                    {props.bookmarkNode}
                </div>
            </div>
        </div>
    )
}