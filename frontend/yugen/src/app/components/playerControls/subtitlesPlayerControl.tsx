"use client"

import { selectTime, usePlayer } from "@videojs/react";
import { Dispatch, SetStateAction, useEffect, useState } from "react";

import "./subtitlesPlayerControl.css"

interface Cue {
    start: number;
    end: number;
    text: string;
}

interface Props {
    url: string | undefined,
    offset: number,
    setOffset: Dispatch<SetStateAction<number>>,

    viewLogs: boolean,
    setViewLogs: Dispatch<SetStateAction<boolean>>
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

        function vttToHtml(text: string): string {
            return text
                .replace(/<i>([\s\S]*?)<\/i>/g, "<em>$1</em>")
                .replace(/<b>([\s\S]*?)<\/b>/g, "<strong>$1</strong>")
                .replace(/<u>([\s\S]*?)<\/u>/g, "<u>$1</u>")
                .replace(/<c[^>]*>([\s\S]*?)<\/c>/g, "$1")
                .replace(/<v[^>]*>([\s\S]*?)<\/v>/g, "$1")
                .replace(/<ruby>([\s\S]*?)<\/ruby>/g, "$1")
                .replace(/<rt>[\s\S]*?<\/rt>/g, "")
                .replace(/<\d{2}:\d{2}[^>]*>/g, "")
                .replace(/&amp;/g, "&")
                .replace(/&lt;/g, "<")
                .replace(/&gt;/g, ">")
                .replace(/&nbsp;/g, " ")
                .trim();
        }

        function isValidSubtitleText(text: string) {
            if (text === "")
                return false;

            const cleaned = text.replace(/\{[^}]*\}/g, "").trim();

            if (!cleaned) return false;
            if (/^\{.*\}$/.test(text)) return false;
            if (/\\(kf|bord|shad|t\(|an|pos|move)/i.test(text)) return false;

            return true;
        }

        for (const block of blocks) {
            const lines = block.trim().split("\n");

            const timingIndex = lines.findIndex(line =>
                line.includes("-->")
            );

            if (timingIndex === -1)
                continue;

            const timingLine = lines[timingIndex];
            const [startRaw, endRaw] = timingLine.split("-->").map(s => s.trim().split(" ")[0]);
            const text = vttToHtml(lines.slice(timingIndex + 1).join("\n"));

            if (!isValidSubtitleText(text))
                continue;

            cues.push({
                start: parseTimestamp(startRaw),
                end: parseTimestamp(endRaw),
                text,
            });
        }

        SetCues(cues.sort((a, b) => a.start - b.start));
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

    }, [props.url])

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
    }, [cues, player]);

    if (cues === undefined)
        return <></>;

    if (props.viewLogs) {
        const alignOffset = (cue: Cue) => {
            props.setOffset(cue.start - (player?.currentTime ?? 0));
        }

        return <div className="VideoPlayer_SubtitlesContainer_Logs" onClick={e => { e.stopPropagation(); props.setViewLogs(false); }}>
            {
                cues.map((c, i) => <div className={lastCue === i ? "Selected" : ""} key={i} dangerouslySetInnerHTML={{ __html: c.text ?? "" }} onClick={e => { e.stopPropagation(); alignOffset(c); }} />)
            }
        </div>
    }

    return (
        <div className="VideoPlayer_SubtitlesContainer">
            <div dangerouslySetInnerHTML={{ __html: currentCue?.text ?? "" }}></div>
        </div>
    )
}