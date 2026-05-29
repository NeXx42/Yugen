import { Playback_Info, PlaybackInfo_Segment } from "@/app/shared/types";
import { selectTime, usePlayer } from "@videojs/react";

import "./segmentSkipperPlayerControl.css"
import { RefObject } from "react";

export default function ({ video, info }: { video: RefObject<HTMLVideoElement | null>, info: Playback_Info }) {
    const time = usePlayer(selectTime);

    const skipSegment = (pos: number) => {
        const segmentEndPercentage = (info.segments[pos].start + info.segments[pos].duration) / 100;
        const segmentEndSeconds = time!.duration * segmentEndPercentage;

        if (video.current)
            video.current.currentTime = segmentEndSeconds;
    }

    if (info?.segments?.length > 0 && time?.currentTime && time.duration) {
        const timePercentage = (time?.currentTime / time?.duration) * 100;

        for (let i = 0; i < info.segments.length; i++) {
            if (timePercentage >= info.segments[i].start && timePercentage <= info.segments[i].start + info.segments[i].duration) {
                return <button className="SegmentSkipper" onClick={() => skipSegment(i)}>SKIP</button>
            }
        }
    }

    return <></>
}