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

    return <button onClick={openSubtitlesEditor}>Edit</button>
}