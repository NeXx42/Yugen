"use client"

import * as api from "@lib/api.local"

import { ReactNode, SubmitEvent, useEffect, useRef, useState } from "react"

import { MediaEpisodeInfo, MediaInfo, Playback_Info } from "@shared/types"

import { audioFeatures, BufferingIndicator, Container, createPlayer, Gesture, videoFeatures } from '@videojs/react';
import {
    Controls,
    PlayButton,
    TimeSlider,
    FullscreenButton,
} from "@videojs/react";
import { Video } from '@videojs/react/video';

import "./mediaPlayer.css"

import VolumePlayerControl from "@/app/components/playerControls/volumePlayerControl";
import SubtitleSelectorPlayerControl from "@/app/components/playerControls/subtitleSelectorPlayerControl";
import SubtitlesPlayerControl from "@/app/components/playerControls/subtitlesPlayerControl";
import WatchtimeSyncerPlayerControl from "@/app/components/playerControls/watchtimeSyncerPlayerControl";
import { createPortal } from "react-dom";
import { useModals } from "@/app/context/modalContext";
import { HlsVideo } from "@videojs/react/media/hls-video";

interface Props {
    mediaInfo: MediaInfo
    episode: MediaEpisodeInfo | undefined
}

const Player = createPlayer({
    features: [
        ...videoFeatures,
    ]
});


export default function (props: Props) {
    const { showModal, closeModal } = useModals();

    const videoRef = useRef<HTMLVideoElement | null>(null);
    const thumbnail = props.episode?.thumbnail ?? props.mediaInfo.thumbnailImage;

    const [selectedSub, setSelectedSub] = useState<number>(-1);
    const [subtitleOffset, setSubtitleOffset] = useState<number>(0);

    const [playbackInfo, setPlaybackInfo] = useState<Playback_Info | undefined>(undefined);
    const [isPlaying, setIsPlaying] = useState(false);
    const [hlsPlayback, setHlsPlayback] = useState(true);

    useEffect(() => fetchPlaybackInfo(), [props.mediaInfo, props.episode])
    useEffect(() => {
        if (videoRef.current == null)
            return;

        for (let i = 0; i < videoRef.current!.textTracks.length; i++) {
            videoRef.current!.textTracks[i].mode = selectedSub === i ? "showing" : "disabled";
        }

    }, [videoRef.current, selectedSub])

    const fetchPlaybackInfo = () => {
        setIsPlaying(false);

        if (props.episode?.jellyfinId) {
            api.media_PlaybackInfo(props.mediaInfo.id, props.episode.number, props.episode?.jellyfinId).then(setPlaybackInfo).catch(() => setPlaybackInfo(undefined));
        }
        else {
            setPlaybackInfo(undefined);
        }
    }

    const onMetadataLoad = (video: HTMLVideoElement) => {
        if (playbackInfo?.historicalTicks == null)
            return;

        video.currentTime = playbackInfo!.historicalTicks / 10_000_000;
    };

    const syncPlaybackTime = (runtime: number, percentage: number) => {
        void api.media_UpdateEpisodeTime(props.mediaInfo.id, props.episode!.number, runtime, percentage);
    }



    const drawSubtitlesEditor = (playbackInfo: Playback_Info) => {
        const uploadSubtitle = async (e: SubmitEvent<HTMLFormElement>) => {
            e.preventDefault();
            e.stopPropagation();

            const form = e.currentTarget;
            const formData = new FormData(form);
            const lang = formData.get("language") as string;

            await api.media_UploadSubtitle(
                playbackInfo.jellyfinId,
                lang,
                formData
            );

            fetchPlaybackInfo();
            closeModal();
        };

        const deleteExternalSubtitle = async (id: number) => {
            await api.media_DeleteSubtitle(playbackInfo.jellyfinId, id);

            fetchPlaybackInfo();
            closeModal();
        }

        const subs = playbackInfo.sources[0].subs.filter(s => s.isExternal);

        showModal(
            <div className="MediaPlayer_SubtitlesEdit">
                <h2>Episode {props.episode?.number}</h2>
                <form onSubmit={uploadSubtitle}>
                    <div>
                        <select name="language">
                            <option value="eng">English</option>
                            <option value="spa">Spanish</option>
                            <option value="fra">French</option>
                            <option value="deu">German</option>
                            <option value="ita">Italian</option>
                            <option value="por">Portuguese</option>
                            <option value="nld">Dutch</option>
                            <option value="swe">Swedish</option>
                            <option value="nor">Norwegian</option>
                            <option value="dan">Danish</option>
                            <option value="fin">Finnish</option>
                            <option value="pol">Polish</option>
                            <option value="rus">Russian</option>
                            <option value="tur">Turkish</option>
                            <option value="ara">Arabic</option>
                            <option value="zho">Chinese</option>
                            <option value="jpn">Japanese</option>
                            <option value="kor">Korean</option>
                        </select>
                        <input type="file" name="subtitle" accept=".srt,.vtt,.ass,.ssa" />
                    </div>

                    <button type="submit">Upload</button>
                </form>

                <div>
                    {
                        subs.length > 0 ? (
                            <table>
                                <thead>
                                    <tr>
                                        <th>Title</th>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {
                                        subs.map(s => < tr key={s.id}>
                                            <td>
                                                {s.title}
                                            </td>
                                            <td className="MediaPlayer_SubtitlesEdit_Existing_Action">
                                                <button onClick={() => deleteExternalSubtitle(s.id)}>Delete</button>
                                            </td>
                                        </tr>)
                                    }
                                </tbody>
                            </table>
                        ) :
                            (
                                <p>No External Subs</p>
                            )
                    }
                </div>
            </div>
        );
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


    const getPlaybackUrl = (info: Playback_Info, hls: boolean) => {
        if (hls) {
            const { videoCodecs, audioCodecs } = detectPlaybackCapabilities();
            return `api/media/${info.jellyfinId}/${0}/stream.m3u8?videoCodecs=${videoCodecs}&audioCodecs=${audioCodecs}`;
        }

        return `api/media/${info.jellyfinId}/${0}/stream.mkv`;
    }

    const drawPlayer = (info: Playback_Info): ReactNode => {
        const source = info.sources[0];

        return (
            <Player.Provider>
                <Container className="VideoPlayer_Container">
                    {
                        hlsPlayback ? <HlsVideo src={getPlaybackUrl(info, true)} ref={videoRef} playsInline autoPlay className="VideoPlayer_Video" onLoadedMetadata={(e) => onMetadataLoad(e.currentTarget)} />
                            : <Video src={getPlaybackUrl(info, false)} ref={videoRef} playsInline autoPlay className="VideoPlayer_Video" onLoadedMetadata={(e) => onMetadataLoad(e.currentTarget)} />
                    }


                    <BufferingIndicator className="VideoPlayer_Buffering" />
                    <Gesture action="togglePaused" type="tap" />
                    <Gesture action="toggleFullscreen" type="doubletap" />

                    <WatchtimeSyncerPlayerControl syncFunc={syncPlaybackTime} />
                    <SubtitlesPlayerControl url={source.subs[selectedSub]?.uri} offset={subtitleOffset} />

                    <Controls.Root className="VideoPlayer_Controls">
                        <TimeSlider.Root className="VideoPlayer_Controls_TimeSlider">
                            <TimeSlider.Track className="VideoPlayer_Controls_TimeSlider_track">
                                <TimeSlider.Buffer className="VideoPlayer_Controls_TimeSlider_buffer" />

                                {
                                    info.segments.map((s, i) => <div className="VideoPlayer_Controls_TimeSlider_Segment" style={{ left: `${s.start}%`, width: `${s.duration}%` }} key={i} />)
                                }

                                <TimeSlider.Fill className="VideoPlayer_Controls_TimeSlider_fill" />
                                <TimeSlider.Thumb className="VideoPlayer_Controls_TimeSlider_thumb" />
                            </TimeSlider.Track>
                        </TimeSlider.Root>

                        <Controls.Group className="VideoPlayer_Controls_Bottom">
                            <div className="VideoPlayer_Controls_Left">
                                <PlayButton className="VideoPlayer_Controls_Play">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" aria-hidden="true" viewBox="0 0 18 18" className="VideoPlayer_Controls_Play_Pause">
                                        <rect width="5" height="14" x="2" y="2" rx="1.75" />
                                        <rect width="5" height="14" x="11" y="2" rx="1.75" />
                                    </svg>
                                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" aria-hidden="true" viewBox="0 0 18 18" className="VideoPlayer_Controls_Play_Play">
                                        <path d="m14.051 10.723-7.985 4.964a1.98 1.98 0 0 1-2.758-.638A2.06 2.06 0 0 1 3 13.964V4.036C3 2.91 3.895 2 5 2c.377 0 .747.109 1.066.313l7.985 4.964a2.057 2.057 0 0 1 .627 2.808c-.16.257-.373.475-.627.637" />
                                    </svg>
                                </PlayButton>
                                <VolumePlayerControl />
                            </div>

                            <div className="VideoPlayer_Controls_Right">
                                <SubtitleSelectorPlayerControl
                                    selectedSub={selectedSub}
                                    selectSub={setSelectedSub}
                                    subtitleOffset={subtitleOffset}
                                    setSubtitleOffset={setSubtitleOffset}
                                    subs={source.subs}
                                />
                                <FullscreenButton className="VideoPlayer_Controls_Fullscreen">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" aria-hidden="true" viewBox="0 0 18 18" className="VideoPlayer_Controls_Fullscreen_Activate">
                                        <path d="M9.57 3.617A1 1 0 0 0 8.646 3H4c-.552 0-1 .449-1 1v4.646a.996.996 0 0 0 1.001 1 1 1 0 0 0 .706-.293l4.647-4.647a1 1 0 0 0 .216-1.089m4.812 4.812a1 1 0 0 0-1.089.217l-4.647 4.647a.998.998 0 0 0 .708 1.706H14c.552 0 1-.449 1-1V9.353a1 1 0 0 0-.618-.924" />
                                    </svg>
                                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="currentColor" aria-hidden="true" viewBox="0 0 18 18" className="VideoPlayer_Controls_Fullscreen_Deactivate">
                                        <path d="M7.883 1.93a.99.99 0 0 0-1.09.217L2.146 6.793A.998.998 0 0 0 2.853 8.5H7.5c.551 0 1-.449 1-1V2.854a1 1 0 0 0-.617-.924m7.263 7.57H10.5c-.551 0-1 .449-1 1v4.646a.996.996 0 0 0 1.001 1.001 1 1 0 0 0 .706-.293l4.646-4.646a.998.998 0 0 0-.707-1.707z" />
                                    </svg>
                                </FullscreenButton>
                            </div>

                            {/*             
                            <VolumeSlider.Root className="VideoPlayer_Controls_VolumeSlider">
                                <VolumeSlider.Track className="VideoPlayer_Controls_VolumeSlider_track">
                                    <VolumeSlider.Fill className="VideoPlayer_Controls_VolumeSlider_fill" />
                                    <VolumeSlider.Thumb className="VideoPlayer_Controls_VolumeSlider_thumb" />
                                </VolumeSlider.Track>
                            </VolumeSlider.Root>                
                            */}
                        </Controls.Group>

                    </Controls.Root>
                </Container>
            </Player.Provider >
        )
    }

    return (
        <div className="MediaPlayer">
            <div className="MediaPlayer_Container">
                {playbackInfo != undefined && isPlaying ? (
                    drawPlayer(playbackInfo)
                ) :
                    (
                        <div className="MediaPlayer_Container_Request" onClick={() => setIsPlaying(true)}>
                            {thumbnail != undefined && <img src={thumbnail ?? ""} />}

                            {
                                playbackInfo == undefined ? (
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="1em"
                                        height="1em"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        stroke="currentColor"
                                        strokeWidth="2"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                    >
                                        {/* Arrow */}
                                        <path d="M12 3v12" />
                                        <path d="M7 10l5 5 5-5" />

                                        {/* Tray */}
                                        <path d="M5 21h14" />
                                    </svg>
                                ) :
                                    (
                                        <svg stroke="currentColor" fill="currentColor" viewBox="0 0 448 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                                            <path d="M424.4 214.7L72.4 6.6C43.8-10.3 0 6.1 0 47.9V464c0 37.5 40.7 60.1 72.4 41.3l352-208c31.4-18.5 31.5-64.1 0-82.6z"></path>
                                        </svg>
                                    )
                            }
                        </div>
                    )
                }
            </div>
            <div className="MediaPlayer_Controls ViewPageContainer">
                <div className="MediaPlayer_Controls_ExternalControls">
                    {playbackInfo && (<>
                        <button onClick={() => drawSubtitlesEditor(playbackInfo)}>Subtitles</button>
                        <div>
                            <a>HLS</a>
                            <input checked={hlsPlayback} onChange={e => setHlsPlayback(e.currentTarget.checked)} type="checkbox" />
                        </div>
                    </>)}
                </div>
            </div>
        </div >
    )
}