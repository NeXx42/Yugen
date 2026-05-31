import { selectTime, usePlayer } from "@videojs/react"
import "./timePlayerControl.css"

export default function () {
    const time = usePlayer(selectTime);

    const currentTimeTxt = formatTime(time?.currentTime ?? 0);
    const durationTxt = formatTime(time?.duration ?? 0);

    function formatTime(seconds: number) {
        seconds = Math.abs(seconds);

        const hours = Math.floor(seconds / 3600);
        const mins = Math.floor((seconds % 3600) / 60);
        const secs = Math.round(seconds % 60);

        const pad = (n: number) => n < 10 ? "0" + n : n;

        if (hours > 0) {
            return `${hours}:${pad(mins)}:${pad(secs)}`;
        } else {
            return `${pad(mins)}:${pad(secs)}`;
        }
    }

    return (
        <a className="TimePlayerControl">{currentTimeTxt} / {durationTxt}</a>
    )
}