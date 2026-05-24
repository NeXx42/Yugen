import { useRef, useState, useEffect, useCallback } from "react";
import { Container, createPlayer } from "@videojs/react";
import "./videoPlayer.css";


const Player = createPlayer({
    features: []
});


function fmt(s: number) {
    const m = Math.floor(s / 60);
    const ss = Math.floor(s % 60).toString().padStart(2, "0");
    return `${m}:${ss}`;
}

function PlayerInner({ src, children }: { src: string; children?: React.ReactNode }) {
    const videoRef = useRef<HTMLVideoElement>(null);
    const [paused, setPaused] = useState(true);
    const [muted, setMuted] = useState(false);
    const [current, setCurrent] = useState(0);
    const [duration, setDuration] = useState(0);

    useEffect(() => {
        const v = videoRef.current;
        if (!v) return;
        const onTime = () => setCurrent(v.currentTime);
        const onMeta = () => setDuration(v.duration);
        const onPlay = () => setPaused(false);
        const onPause = () => setPaused(true);
        v.addEventListener("timeupdate", onTime);
        v.addEventListener("loadedmetadata", onMeta);
        v.addEventListener("play", onPlay);
        v.addEventListener("pause", onPause);
        return () => {
            v.removeEventListener("timeupdate", onTime);
            v.removeEventListener("loadedmetadata", onMeta);
            v.removeEventListener("play", onPlay);
            v.removeEventListener("pause", onPause);
        };
    }, []);

    const togglePlay = () => videoRef.current?.paused
        ? videoRef.current.play()
        : videoRef.current?.pause();

    const toggleMute = () => {
        if (!videoRef.current) return;
        videoRef.current.muted = !videoRef.current.muted;
        setMuted(videoRef.current.muted);
    };

    const seek = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (!videoRef.current) return;
        videoRef.current.currentTime = Number(e.target.value);
    };

    return (
        <Container className="player-root">
            <video ref={videoRef} src={src} style={{ width: "100%", display: "block" }}>
                {children}
            </video>

            <div className="player-controls">
                {/* Progress bar */}
                <input
                    className="progress"
                    type="range"
                    min={0}
                    max={duration || 100}
                    step={0.1}
                    value={current}
                    onChange={seek}
                />

                {/* Button row */}
                <div className="controls-row">
                    <button className="ctrl-btn" onClick={togglePlay}>
                        {paused ? "▶" : "⏸"}
                    </button>
                    <button className="ctrl-btn" onClick={toggleMute}>
                        {muted ? "🔇" : "🔊"}
                    </button>

                    {/* 👇 drop your custom element here */}

                    <span className="time">{fmt(current)} / {fmt(duration)}</span>
                </div>
            </div>
        </Container>
    );
}

export default function VideoPlayer({ src, subs }: {
    src: string;
    subs: { uri: string; language: string }[];
}) {
    return (
        <Player.Provider>
            <PlayerInner src={src}>
                {subs.map(s => (
                    <track key={s.language} src={s.uri} label={s.language} srcLang={s.language} />
                ))}
            </PlayerInner>
        </Player.Provider>
    );
}