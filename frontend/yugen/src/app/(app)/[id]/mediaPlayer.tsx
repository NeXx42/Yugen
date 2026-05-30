"use client"

import * as api from "@lib/api.local"

import { ReactNode, SubmitEvent, useEffect, useRef, useState } from "react"

import { MediaEpisodeInfo, MediaInfo, Playback_Info } from "@shared/types"

import "./mediaPlayer.css"
import { useModals } from "@/app/context/modalContext";
import PlayerControl from "@/app/components/playerControls/playerControl"

interface Props {
    mediaInfo: MediaInfo
    episode: MediaEpisodeInfo | undefined
}

export default function (props: Props) {
    const { showModal, closeModal } = useModals();

    const thumbnail = props.episode?.thumbnail ?? props.mediaInfo.thumbnailImage;


    const [playbackInfo, setPlaybackInfo] = useState<Playback_Info | undefined>(undefined);
    const [isPlaying, setIsPlaying] = useState(false);


    useEffect(() => fetchPlaybackInfo(), [props.mediaInfo, props.episode])


    const fetchPlaybackInfo = () => {
        setIsPlaying(false);

        if (props.episode?.jellyfinId) {
            api.media_PlaybackInfo(props.mediaInfo.id, props.episode.number, props.episode?.jellyfinId).then(setPlaybackInfo).catch(() => setPlaybackInfo(undefined));
        }
        else {
            setPlaybackInfo(undefined);
        }
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






    return (
        <div className="MediaPlayer">
            <div className="MediaPlayer_Container">
                {playbackInfo != undefined && isPlaying ? (
                    <PlayerControl mediaInfo={props.mediaInfo} episodeInfo={props.episode!} playbackInfo={playbackInfo} />
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
                    </>)}
                </div>
            </div>
        </div >
    )
}