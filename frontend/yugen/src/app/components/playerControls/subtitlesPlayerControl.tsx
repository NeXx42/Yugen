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
                .replace(/\{[^}]*\}/g, "")
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

            if (!cleaned)
                return false;

            if (/\\(kf|bord|shad|t\(|an|pos|move)/i.test(cleaned)) return false;
            return true;
        }

        function mergeOverlappingCues(cues: Cue[]): Cue[] {
            if (cues.length <= 1)
                return cues;

            const boundaries = [
                ...new Set(cues.flatMap(cue => [cue.start, cue.end]))
            ].sort((a, b) => a - b);

            const result: Cue[] = [];

            for (let i = 0; i < boundaries.length - 1; i++) {
                const start = boundaries[i];
                const end = boundaries[i + 1];
                const activeCues = cues.filter(cue => cue.start <= start && cue.end >= end);

                if (activeCues.length === 0)
                    continue;

                result.push({ ...activeCues[0], start, end, text: activeCues.map(cue => cue.text).join("\n"), });
            }

            return result;
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

        SetCues(mergeOverlappingCues(cues));
    }

    useEffect(() => {
        if (props.url === undefined) {
            SetCues(undefined);
            return;
        }

        fetch(props.url, {
            method: "GET",
            credentials: "include",
        }).then((r) => r.text().then(parseVtt));

    }, [props.url])

    if (cues === undefined)
        return <></>;

    function findCue(time: number): Cue | undefined {
        if (cues === undefined)
            return undefined;

        for (let i = 0; i < cues.length; i++) {
            if (time < cues[i].start) return undefined; // no cue can match from here on
            if (time <= cues[i].end) return cues[i];     // start <= time is implied by above
        }
        return undefined;
    }

    const currentCue = findCue((player?.currentTime ?? 0) + props.offset);

    if (props.viewLogs) {
        const alignOffset = (cue: Cue) => {
            props.setOffset(cue.start - (player?.currentTime ?? 0));
        }

        return <div className="VideoPlayer_SubtitlesContainer_Logs" onClick={e => { e.stopPropagation(); props.setViewLogs(false); }}>
            {
                cues.map((c, i) => <div className={currentCue?.start === c.start ? "Selected" : ""} key={i} dangerouslySetInnerHTML={{ __html: c.text ?? "" }} onClick={e => { e.stopPropagation(); alignOffset(c); }} />)
            }
        </div>
    }

    return (
        <div className="VideoPlayer_SubtitlesContainer">
            <div dangerouslySetInnerHTML={{ __html: currentCue?.text ?? "" }}></div>
        </div>
    )
}