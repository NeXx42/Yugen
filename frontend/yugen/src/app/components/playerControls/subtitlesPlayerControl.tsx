"use client"

import { selectTime, usePlayer } from "@videojs/react";
import { useEffect, useState } from "react";

import "./subtitlesPlayerControl.css"

interface Cue {
    start: number;
    end: number;
    text: string;
}

interface Props {
    url: string | undefined,
    offset: number
}

export default function (props: Props) {
    const player = usePlayer(selectTime);

    const [lastCue, setLastCue] = useState<number | undefined>(undefined);
    const [currentCue, setCurrentCue] = useState<Cue | undefined>(undefined);

    const [cues, SetCues] = useState<Cue[] | undefined>(undefined);

    function parseTimestamp(ts: string): number {
        const parts = ts.split(":");

        if (parts.length === 3) {
            const [h, m, s] = parts;
            return (
                Number(h) * 3600 +
                Number(m) * 60 +
                Number(s)
            );
        }

        const [m, s] = parts;
        return Number(m) * 60 + Number(s);
    }

    function parseVtt(vtt: string) {
        const blocks = vtt.replace(/\r/g, "").split(/\n\s*\n/);
        const cues: Cue[] = [];

        for (const block of blocks) {
            const lines = block.trim().split("\n");

            const timingIndex = lines.findIndex(line =>
                line.includes("-->")
            );

            if (timingIndex === -1)
                continue;

            const timingLine = lines[timingIndex];
            const [startRaw, endRaw] = timingLine.split("-->").map(s => s.trim().split(" ")[0]);
            const text = lines.slice(timingIndex + 1).join("\n");

            cues.push({
                start: parseTimestamp(startRaw),
                end: parseTimestamp(endRaw),
                text,
            });
        }

        SetCues(cues);
    }

    function WithinCue(time: number, pos: number): boolean {
        return time >= cues![pos].start && time <= cues![pos].end;
    }

    useEffect(() => {
        setLastCue(undefined);
        setCurrentCue(undefined);

        if (props.url === undefined) {
            SetCues(undefined);
            return;
        }

        fetch(props.url, {
            method: "GET",
            credentials: "include",
        }).then((r) => r.text().then(parseVtt));

    }, [props])

    useEffect(() => {
        const interval = setInterval(() => {
            if (!cues?.length)
                return;

            const t = (player?.currentTime ?? 0) + props.offset;

            if (lastCue != undefined && WithinCue(t, lastCue)) {
                return;
            }

            let i = lastCue ?? 0;

            while (i > 0 && t < cues[i].start) i--;
            while (i < cues.length && t > cues[i].end) i++;

            if (WithinCue(t, i)) {
                setLastCue(i);
                setCurrentCue(cues[i]);
            }
            else {
                setCurrentCue(undefined);
            }

        }, 10);

        return () => clearInterval(interval);
    }, [cues, currentCue, player]);

    if (cues === undefined)
        return <></>;

    return (
        <div className="VideoPlayer_SubtitlesContainer">
            <div>{currentCue?.text}</div>
        </div>
    )
}