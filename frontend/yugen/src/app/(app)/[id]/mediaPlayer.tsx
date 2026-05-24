"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useRef, useState } from "react"

import { MediaEpisodeInfo, MediaInfo, Playback_Info, Playback_Info_Subtitle } from "@shared/types"

import { BufferingIndicator, Container, createPlayer, Gesture, videoFeatures } from '@videojs/react';
import {
    Controls,
    PlayButton,
    MuteButton,
    TimeSlider,
    FullscreenButton,
    VolumeSlider,
} from "@videojs/react";
import { Video } from '@videojs/react/video';

import "./mediaPlayer.css"
import SubtitlePlayerControl from "@/app/components/playerControls/subtitlePlayerControl";
import VolumePlayerControl from "@/app/components/playerControls/volumePlayerControl";

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
    const videoRef = useRef<HTMLVideoElement | null>(null);

    const thumbnail = props.episode?.thumbnail ?? props.mediaInfo.thumbnailImage;

    const [selectedSub, setSelectedSub] = useState<Playback_Info_Subtitle | undefined>(undefined);

    const [playbackInfo, setPlaybackInfo] = useState<Playback_Info | undefined>(undefined);
    const [isPlaying, setIsPlaying] = useState(false);

    useEffect(() => {
        setIsPlaying(false);

        if (props.episode?.jellyfinId) {
            api.media_PlaybackInfo(props.episode?.jellyfinId).then(setPlaybackInfo).catch(() => setPlaybackInfo(undefined));
        }
        else {
            setPlaybackInfo(undefined);
        }

    }, [props.mediaInfo, props.episode])

    const test = () => {
        for (let i = 0; i < videoRef.current!.textTracks.length; i++) {
            videoRef.current!.textTracks[i].mode = "showing";
        }
    }

    const drawPlayer = (info: Playback_Info): ReactNode => {
        const source = info.sources[0];

        return (
            <Player.Provider>
                <Container className="VideoPlayer_Container">
                    <Video ref={videoRef} src={`api/media/${info.jellyfinId}/stream.mkv?mediaId=${source.id}`} playsInline autoPlay className="VideoPlayer_Video">
                        {source.subs.map(s => <track src={s.uri} label={s.language} srcLang={s.language} />)}
                    </Video>

                    <BufferingIndicator className="VideoPlayer_Buffering" />
                    <Gesture action="togglePaused" type="tap" />
                    <Gesture action="toggleFullscreen" type="doubletap" />


                    <Controls.Root className="VideoPlayer_Controls">
                        <TimeSlider.Root className="VideoPlayer_Controls_TimeSlider">
                            <TimeSlider.Track className="VideoPlayer_Controls_TimeSlider_track">
                                <TimeSlider.Buffer className="VideoPlayer_Controls_TimeSlider_buffer" />
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
                                <SubtitlePlayerControl videoElement={videoRef.current!} />
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

                            <svg stroke="currentColor" fill="currentColor" viewBox="0 0 448 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                                <path d="M424.4 214.7L72.4 6.6C43.8-10.3 0 6.1 0 47.9V464c0 37.5 40.7 60.1 72.4 41.3l352-208c31.4-18.5 31.5-64.1 0-82.6z"></path>
                            </svg>
                        </div>
                    )
                }
            </div>
            <div className="MediaPlayer_Controls ViewPageContainer">
                <div className="MediaPlayer_Controls_Bookmark">
                    {playbackInfo && (<>
                        {playbackInfo.sources[0].id}
                        <select>
                            {playbackInfo.sources[0].subs.map(e => <option>{e.language}</option>)}
                        </select>
                        <button onClick={test} >test</button>
                    </>)}
                </div>
            </div>
        </div >
    )
}