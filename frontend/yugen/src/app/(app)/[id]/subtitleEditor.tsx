import * as api from "@lib/api.local"

import { MediaEpisodeInfo, MediaInfo, Playback_Info } from "@/app/shared/types";
import { useModals } from "@/app/context/modalContext";
import { ReactNode, SubmitEvent, useEffect, useRef, useState } from "react"

import "./subtitleEditor.css"

interface Props {
    mediaInfo: MediaInfo,
    episodeInfo: MediaEpisodeInfo,
    playbackInfo: Playback_Info,

    onUpdate: () => void
}

export default function (props: Props) {
    const { showModal, closeModal } = useModals();

    const openSubtitlesEditor = () => {
        const uploadSubtitle = async (e: SubmitEvent<HTMLFormElement>) => {
            e.preventDefault();
            e.stopPropagation();

            const form = e.currentTarget;
            const formData = new FormData(form);
            const lang = formData.get("language") as string;

            await api.media_UploadSubtitle(
                props.playbackInfo.jellyfinId,
                lang,
                formData
            );

            props.onUpdate?.();
            closeModal();
        };

        const deleteExternalSubtitle = async (id: number) => {
            await api.media_DeleteSubtitle(props.playbackInfo.jellyfinId, id);

            props.onUpdate?.();
            closeModal();
        }

        const subs = props.playbackInfo.sources[0].subs.filter(s => s.isExternal);

        showModal(
            <div className="MediaPlayer_SubtitlesEdit">
                <h2>Episode {props.episodeInfo?.number}</h2>
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
        <button onClick={openSubtitlesEditor}>
            <svg stroke="currentColor" fill="currentColor" stroke-width="0" viewBox="0 0 24 24" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                <path d="M21 3C21.5523 3 22 3.44772 22 4V20C22 20.5523 21.5523 21 21 21H3C2.44772 21 2 20.5523 2 20V4C2 3.44772 2.44772 3 3 3H21ZM9 8C6.792 8 5 9.792 5 12C5 14.208 6.792 16 9 16C10.1 16 11.1 15.55 11.828 14.828L10.4144 13.4144C10.0525 13.7762 9.5525 14 9 14C7.895 14 7 13.105 7 12C7 10.895 7.895 10 9 10C9.55 10 10.0483 10.22 10.4153 10.5866L11.829 9.173C11.1049 8.44841 10.1045 8 9 8ZM16 8C13.792 8 12 9.792 12 12C12 14.208 13.792 16 16 16C17.104 16 18.104 15.552 18.828 14.828L17.4144 13.4144C17.0525 13.7762 16.5525 14 16 14C14.895 14 14 13.105 14 12C14 10.895 14.895 10 16 10C16.553 10 17.0534 10.2241 17.4153 10.5866L18.829 9.173C18.1049 8.44841 17.1045 8 16 8Z" />
            </svg>
            Edit subtitles
        </button>)
}