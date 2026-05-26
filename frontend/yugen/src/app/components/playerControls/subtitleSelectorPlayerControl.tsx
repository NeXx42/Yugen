import { selectTextTrack, usePlayer } from "@videojs/react";
import { Dispatch, ReactNode, SetStateAction, useEffect, useState } from "react";

import "./subtitleSelectorPlayerControl.css"
import { Playback_Info_Subtitle } from "@/app/shared/types";

interface Props {
    selectedSub: number,
    selectSub: Dispatch<SetStateAction<number>>

    subtitleOffset: number,
    setSubtitleOffset: Dispatch<SetStateAction<number>>

    subs: Playback_Info_Subtitle[]
}

export default function (props: Props): ReactNode {
    const [isHovered, setIsHovered] = useState(false);
    const [editSubtitleOffset, setEditSubtitleOffset] = useState(false);

    const selectSub = (id: number) => {
        props.selectSub(id);
        setIsHovered(false);
    }

    return (<button onClick={() => setIsHovered(true)} className="MediaControl_Subtitles">
        Subs

        {isHovered && (
            <div className="MediaControl_Subtitles_Popup" onMouseLeave={() => setIsHovered(false)} >
                <div className="MediaControl_Subtitles_Popup_SliderContainer" onClick={e => e.stopPropagation()}>
                    <div onClick={() => setEditSubtitleOffset(true)}>Edit</div>

                    <div onClick={() => selectSub(-1)} className={props.selectedSub === -1 ? "Selected" : ""}>None</div>
                    {props.subs.map((t, i) => <div key={i} onClick={() => selectSub(i)} className={props.selectedSub === i ? "Selected" : ""}>{t.title}</div>)}
                </div>
            </div>
        )}
    </button>)
}