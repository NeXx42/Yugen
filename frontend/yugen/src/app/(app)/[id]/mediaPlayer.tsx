import { ReactNode } from "react"

import "./mediaPlayer.css"

interface Props {
    itemId: string | undefined,
    bookmarkNode: ReactNode
}

export default function (props: Props) {
    return (
        <div className="MediaPlayer">
            <div className="MediaPlayer_Container">
                {props.itemId != undefined ? (
                    <iframe
                        src={`https://jellyfin.local/web/index.html#!/details?id=${props.itemId}`}
                        allow="autoplay; fullscreen"
                    ></iframe>
                ) :
                    (
                        <div className="MediaPlayer_Container_Request">
                            <button>Request</button>
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