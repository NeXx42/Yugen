"use client"

import * as api from "@lib/api.local"

import { BufferingIndicator, Container, createPlayer, Gesture, selectPlaybackRate, usePlayer, videoFeatures } from '@videojs/react';
import {
    Controls,
    PlayButton,
    TimeSlider,
    FullscreenButton,
} from "@videojs/react";

import { Video } from '@videojs/react/video';
import { HlsVideo } from "@videojs/react/media/hls-video";

import VolumePlayerControl from "@/app/components/playerControls/volumePlayerControl";
import SubtitlesPlayerControl from "@/app/components/playerControls/subtitlesPlayerControl";
import WatchtimeSyncerPlayerControl from "@/app/components/playerControls/watchtimeSyncerPlayerControl";
import SegmentSkipperPlayerControl from "@/app/components/playerControls/segmentSkipperPlayerControl";
import TimePlayerControl from "@/app/components/playerControls/timePlayerControl";

import { MediaEpisodeInfo, MediaInfo, Playback_Info } from '@/app/shared/types';
import { useEffect, useRef, useState } from 'react';

import "./playerControl.css"
import PlaybackSpeedListPlayerControl from "./playbackSpeedListPlayerControl";

const Player = createPlayer({
    features: [
        ...videoFeatures,
    ]
});


export type MenuType = "None" | "Settings" | "Subtitles" | "Audio";
export type SettingsMenu = "None" | "Quality" | "PlaybackSpeed";


export default function ({ mediaInfo, episodeInfo, playbackInfo }: { mediaInfo: MediaInfo, episodeInfo: MediaEpisodeInfo, playbackInfo: Playback_Info }) {
    const source = playbackInfo.sources[0];
    const videoRef = useRef<HTMLVideoElement | null>(null);

    const [selectedAudio, setSelectedAudio] = useState<number | null>(() => {
        return source.audio.find(a => a.isDefault)?.id ?? null;
    });

    const [viewSubLogs, setViewSubLogs] = useState(false);
    const [selectedSub, setSelectedSub] = useState<number>(-1);
    const [subtitleOffset, setSubtitleOffset] = useState<number>(0);

    const [settingsMenu, setSettingsMenu] = useState<SettingsMenu>("None");
    const [activeSubMenu, setActiveSubMenu] = useState<MenuType>("None");

    const [hlsPlayback, setHlsPlayback] = useState(true);

    useEffect(() => {
        if (videoRef.current == null)
            return;

        for (let i = 0; i < videoRef.current!.textTracks.length; i++) {
            videoRef.current!.textTracks[i].mode = selectedSub === i ? "showing" : "disabled";
        }

    }, [videoRef.current, selectedSub])

    const drawSubMenu = () => {
        const middleMan = (callback: () => void) => {
            callback();
            setActiveSubMenu("None");
        }

        switch (activeSubMenu) {
            case "Subtitles":
                return (<div className="SubMenu_Subs">
                    <div className="SubMenu_Subs_OffsetContainer">
                        <input type="number" value={subtitleOffset} onChange={e => setSubtitleOffset(Number.parseInt(e.target.value))} />
                        <input type="range" min={-20} max={20} value={subtitleOffset} onChange={e => setSubtitleOffset(Number.parseFloat(e.target.value))} step={0.01} />
                        <button onClick={() => setViewSubLogs(true)}>Logs</button>
                    </div>
                    <div className="SubMenu_Subs_Subs SubMenu_Generic_Container">
                        <div onClick={() => setSelectedSub(-1)} className={selectedSub === -1 ? "Selected" : ""}>Off</div>
                        {source.subs.map((t, i) => <div key={i} onClick={() => middleMan(() => setSelectedSub(i))} className={selectedSub === i ? "Selected" : ""}>{t.title}</div>)}
                    </div>
                </div>)

            case "Settings":
                switch (settingsMenu) {
                    default:
                        return (
                            <div className="SubMenu_Options SubMenu_Generic_Container">
                                <button onClick={() => setSettingsMenu("Quality")}>Quality</button>
                                <button onClick={() => setSettingsMenu("PlaybackSpeed")}>Playback Speed</button>
                            </div>
                        )

                    case "PlaybackSpeed":
                        return (
                            <>
                                <button onClick={() => setSettingsMenu("None")}>Back</button>
                                <div className="SubMenu_Options SubMenu_Generic_Container">
                                    <PlaybackSpeedListPlayerControl />
                                </div>
                            </>
                        )

                    case "Quality":
                        return (
                            <>

                                <button onClick={() => setSettingsMenu("None")}>Back</button>
                                <div className="SubMenu_Options SubMenu_Generic_Container">
                                    <button className={!hlsPlayback ? "Selected" : ""} onClick={() => middleMan(() => setHlsPlayback(false))}>Direct Play</button>
                                    <button className={hlsPlayback ? "Selected" : ""} onClick={() => middleMan(() => setHlsPlayback(true))}>HLS</button>
                                </div>
                            </>
                        )
                }



            case "Audio":
                return (
                    <div className="SubMenu_Audio SubMenu_Generic_Container">
                        {
                            source.audio.map(a => <div key={a.id} onClick={() => middleMan(() => setSelectedAudio(a.id))} className={a.id === selectedAudio ? "Selected" : ""}>{a.title}</div>)
                        }
                    </div>
                )
        }

        return <></>;
    }





    const handleVideoClick = () => {
        if (videoRef === null)
            return;

        if (videoRef.current?.paused)
            videoRef?.current?.play();
        else
            videoRef?.current?.pause();
    }

    const onMetadataLoad = (video: HTMLVideoElement) => {
        if (playbackInfo?.historicalTicks == null)
            return;

        video.currentTime = playbackInfo!.historicalTicks / 10_000_000;
    };

    const syncPlaybackTime = (runtime: number, percentage: number) => {
        void api.media_UpdateEpisodeTime(mediaInfo.id, episodeInfo!.number, runtime, percentage);
    }

    const detectPlaybackCapabilities = () => {
        const test = (mime: string) => {
            return window.MediaSource?.isTypeSupported(mime) ?? false;
        };

        let videoCodecs = []
        let audioCodecs = []

        if (test('video/mp4; codecs="avc1.42E01E,mp4a.40.2"')) {
            if (test('video/mp4; codecs="avc1.42E01E"')) videoCodecs.push("h264");
            if (test('video/mp4; codecs="hvc1.1.6.L93.B0"')) videoCodecs.push("hevc");
            if (test('video/mp4; codecs="av01.0.05M.08"')) videoCodecs.push("av1");

            if (test('audio/mp4; codecs="mp4a.40.2"')) audioCodecs.push("aac");
            if (test('audio/mp4; codecs="ac-3"')) audioCodecs.push("ac3");
            if (test('audio/mp4; codecs="ec-3"')) audioCodecs.push("eac3");
        }

        return {
            videoCodecs: videoCodecs.join(","),
            audioCodecs: audioCodecs.join(","),
        }
    }

    const getPlaybackUrl = () => {
        const format = hlsPlayback ? "m3u8" : "mkv";
        let vidParams: string[] = [];

        if (selectedAudio) {
            vidParams.push(`audioStreamIndex=${selectedAudio}`)
        }

        if (hlsPlayback) {
            const { videoCodecs, audioCodecs } = detectPlaybackCapabilities();
            vidParams.push(`videoCodecs=${videoCodecs}`);
            vidParams.push(`audioCodecs=${audioCodecs}`);
        }

        return `api/media/${playbackInfo.jellyfinId}/${0}/stream.${format}?${vidParams.join("&")}`;
    }

    const playbackUrl = getPlaybackUrl();
    const iconSize = 20;

    return (
        <Player.Provider>
            <Container className="VideoPlayer_Container">
                {
                    hlsPlayback ? <HlsVideo src={playbackUrl} ref={videoRef} playsInline autoPlay className="VideoPlayer_Video" onLoadedMetadata={(e) => onMetadataLoad(e.currentTarget)} />
                        : <Video src={playbackUrl} ref={videoRef} playsInline autoPlay className="VideoPlayer_Video" onLoadedMetadata={(e) => onMetadataLoad(e.currentTarget)} />
                }


                <BufferingIndicator className="VideoPlayer_Buffering" />
                <Gesture action="toggleFullscreen" type="doubletap" />

                <WatchtimeSyncerPlayerControl syncFunc={syncPlaybackTime} />
                <SubtitlesPlayerControl url={source.subs[selectedSub]?.uri} offset={subtitleOffset} viewLogs={viewSubLogs} setViewLogs={setViewSubLogs} setOffset={setSubtitleOffset} />

                <SegmentSkipperPlayerControl video={videoRef} info={playbackInfo} />

                <Controls.Root className="VideoPlayer_Controls" onClick={handleVideoClick}>
                    <Controls.Group className="VideoPlayer_Controls_Bottom" onClick={e => e.stopPropagation()}>
                        <TimeSlider.Root className="VideoPlayer_Controls_TimeSlider">
                            <TimeSlider.Track className="VideoPlayer_Controls_TimeSlider_track">
                                <TimeSlider.Buffer className="VideoPlayer_Controls_TimeSlider_buffer" />
                                {playbackInfo.segments.map((s, i) => <div className="VideoPlayer_Controls_TimeSlider_Segment" style={{ left: `${s.start}%`, width: `${s.duration}%` }} key={i} />)}
                                {playbackInfo.chapters.map((s, i) => <div className="VideoPlayer_Controls_TimeSlider_Chapter" style={{ left: `${s}%` }} key={i} />)}
                                <TimeSlider.Fill className="VideoPlayer_Controls_TimeSlider_fill" />
                                <TimeSlider.Thumb className="VideoPlayer_Controls_TimeSlider_thumb" />
                            </TimeSlider.Track>
                        </TimeSlider.Root>

                        <div className="VideoPlayer_Controls_Left">
                            <PlayButton className="VideoPlayer_Controls_Play">
                                <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" aria-hidden="true" viewBox="0 0 18 18" className="VideoPlayer_Controls_Play_Pause">
                                    <rect width="5" height="14" x="2" y="2" rx="1.75" />
                                    <rect width="5" height="14" x="11" y="2" rx="1.75" />
                                </svg>
                                <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" aria-hidden="true" viewBox="0 0 18 18" className="VideoPlayer_Controls_Play_Play">
                                    <path d="m14.051 10.723-7.985 4.964a1.98 1.98 0 0 1-2.758-.638A2.06 2.06 0 0 1 3 13.964V4.036C3 2.91 3.895 2 5 2c.377 0 .747.109 1.066.313l7.985 4.964a2.057 2.057 0 0 1 .627 2.808c-.16.257-.373.475-.627.637" />
                                </svg>
                            </PlayButton>
                            <VolumePlayerControl />
                            <TimePlayerControl />
                        </div>

                        <div className="VideoPlayer_Controls_Right">
                            <FullscreenButton className="VideoPlayer_Controls_Fullscreen">
                                <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" className="VideoPlayer_Controls_Fullscreen_Activate" viewBox="0 0 16 16">
                                    <path fillRule="evenodd" d="M5.828 10.172a.5.5 0 0 0-.707 0l-4.096 4.096V11.5a.5.5 0 0 0-1 0v3.975a.5.5 0 0 0 .5.5H4.5a.5.5 0 0 0 0-1H1.732l4.096-4.096a.5.5 0 0 0 0-.707m4.344 0a.5.5 0 0 1 .707 0l4.096 4.096V11.5a.5.5 0 1 1 1 0v3.975a.5.5 0 0 1-.5.5H11.5a.5.5 0 0 1 0-1h2.768l-4.096-4.096a.5.5 0 0 1 0-.707m0-4.344a.5.5 0 0 0 .707 0l4.096-4.096V4.5a.5.5 0 1 0 1 0V.525a.5.5 0 0 0-.5-.5H11.5a.5.5 0 0 0 0 1h2.768l-4.096 4.096a.5.5 0 0 0 0 .707m-4.344 0a.5.5 0 0 1-.707 0L1.025 1.732V4.5a.5.5 0 0 1-1 0V.525a.5.5 0 0 1 .5-.5H4.5a.5.5 0 0 1 0 1H1.732l4.096 4.096a.5.5 0 0 1 0 .707" />
                                </svg>
                                <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" className="VideoPlayer_Controls_Fullscreen_Deactivate" viewBox="0 0 16 16">
                                    <path d="M5.5 0a.5.5 0 0 1 .5.5v4A1.5 1.5 0 0 1 4.5 6h-4a.5.5 0 0 1 0-1h4a.5.5 0 0 0 .5-.5v-4a.5.5 0 0 1 .5-.5m5 0a.5.5 0 0 1 .5.5v4a.5.5 0 0 0 .5.5h4a.5.5 0 0 1 0 1h-4A1.5 1.5 0 0 1 10 4.5v-4a.5.5 0 0 1 .5-.5M0 10.5a.5.5 0 0 1 .5-.5h4A1.5 1.5 0 0 1 6 11.5v4a.5.5 0 0 1-1 0v-4a.5.5 0 0 0-.5-.5h-4a.5.5 0 0 1-.5-.5m10 1a1.5 1.5 0 0 1 1.5-1.5h4a.5.5 0 0 1 0 1h-4a.5.5 0 0 0-.5.5v4a.5.5 0 0 1-1 0z" />
                                </svg>
                            </FullscreenButton>

                            <button onClick={() => setActiveSubMenu("Settings")}>
                                <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" viewBox="0 0 16 16">
                                    <path d="M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z" />
                                </svg>
                            </button>

                            {
                                source.subs?.length > 0 && (
                                    <button onClick={() => setActiveSubMenu("Subtitles")}>
                                        <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" viewBox="0 0 16 16">
                                            <path d="M2 2a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2zm3.027 4.002c-.83 0-1.319.642-1.319 1.753v.743c0 1.107.48 1.727 1.319 1.727.69 0 1.138-.435 1.186-1.05H7.36v.114c-.057 1.147-1.028 1.938-2.342 1.938-1.613 0-2.518-1.028-2.518-2.729v-.747C2.5 6.051 3.414 5 5.018 5c1.318 0 2.29.813 2.342 2v.11H6.213c-.048-.638-.505-1.108-1.186-1.108m6.14 0c-.831 0-1.319.642-1.319 1.753v.743c0 1.107.48 1.727 1.318 1.727.69 0 1.139-.435 1.187-1.05H13.5v.114c-.057 1.147-1.028 1.938-2.342 1.938-1.613 0-2.518-1.028-2.518-2.729v-.747c0-1.7.914-2.751 2.518-2.751 1.318 0 2.29.813 2.342 2v.11h-1.147c-.048-.638-.505-1.108-1.187-1.108z" />
                                        </svg>
                                    </button>
                                )
                            }

                            {
                                source.audio?.length > 1 && (
                                    <button onClick={() => setActiveSubMenu("Audio")}>
                                        <svg xmlns="http://www.w3.org/2000/svg" width={iconSize} height={iconSize} fill="currentColor" viewBox="0 0 16 16">
                                            <path d="M9 13c0 1.105-1.12 2-2.5 2S4 14.105 4 13s1.12-2 2.5-2 2.5.895 2.5 2" />
                                            <path fillRule="evenodd" d="M9 3v10H8V3z" />
                                            <path d="M8 2.82a1 1 0 0 1 .804-.98l3-.6A1 1 0 0 1 13 2.22V4L8 5z" />
                                        </svg>
                                    </button>
                                )
                            }

                        </div>
                    </Controls.Group>

                    {
                        activeSubMenu !== "None" && (
                            <div className="VideoPlayer_SubMenu_Container" onClick={e => { e.stopPropagation(); setActiveSubMenu("None"); }}>
                                <div className="VideoPlayer_SubMenu" onClick={e => e.stopPropagation()}>
                                    <h2>{activeSubMenu}</h2>
                                    {drawSubMenu()}
                                </div>
                            </div>
                        )
                    }
                </Controls.Root>
            </Container>
        </Player.Provider >
    )
}