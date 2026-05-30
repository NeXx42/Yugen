import { playbackRateFeature, selectPlaybackRate, usePlayer } from "@videojs/react";

export default function () {
    const playback = usePlayer(selectPlaybackRate);

    return (<>
        {playback?.playbackRates.map(r => <div key={r} className={playback.playbackRate === r ? "Selected" : ""} onClick={() => playback.setPlaybackRate(r)} >{r}</div>)}
    </>)
}