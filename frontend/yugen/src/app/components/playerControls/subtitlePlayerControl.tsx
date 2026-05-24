import { selectTextTrack, usePlayer } from "@videojs/react";
import { ReactNode, useEffect, useState } from "react";

import "./subtitlePlayerControl.css"

interface Props {
    videoElement: HTMLVideoElement
}

export default function (props: Props): ReactNode {
    const player = usePlayer(selectTextTrack);
    const [isHovered, setIsHovered] = useState(false);
    const [activeSub, setActiveSub] = useState(-1);

    const changeSub = (to: number) => {
        setActiveSub(to);

        for (let i = 0; i < props.videoElement.textTracks.length; i++) {
            props.videoElement.textTracks[i].mode = i === to ? "showing" : "disabled";
        }
    }

    return (<button onClick={() => setIsHovered(true)} className="MediaControl_Subtitles">
        Subs

        {isHovered && (
            <div className="MediaControl_Subtitles_Popup" onMouseLeave={() => setIsHovered(false)} >
                <div className="MediaControl_Subtitles_Popup_SliderContainer" onClick={e => e.stopPropagation()}>
                    <div onClick={() => setActiveSub(-1)}>None</div>
                    {player?.textTrackList.map((t, i) => <div key={i} onClick={() => changeSub(i)}>{t.label}</div>)}
                </div>
            </div>
        )}
    </button>)
}