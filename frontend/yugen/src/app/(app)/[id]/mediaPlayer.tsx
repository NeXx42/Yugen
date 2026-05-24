"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useState } from "react"

import { MediaEpisodeInfo, MediaInfo, Playback_Info, Playback_Info_Subtitle } from "@shared/types"

import '@videojs/react/video/skin.css';
import { createPlayer, usePlayer, videoFeatures } from '@videojs/react';
import { VideoSkin, Video } from '@videojs/react/video';

import "./mediaPlayer.css"

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

    const drawPlayer = (info: Playback_Info): ReactNode => {
        const source = info.sources[0];

        return (
            <Player.Provider>
                <VideoSkin >
                    <Video src={`api/media/${info.jellyfinId}/stream.mkv?mediaId=${source.id}`} playsInline >
                        {selectedSub != undefined && <track src={selectedSub.uri} label={selectedSub.language} srcLang={selectedSub.language} />}
                    </Video>

                </VideoSkin>
            </Player.Provider>
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
                    </>)}
                </div>
            </div>
        </div >
    )
}