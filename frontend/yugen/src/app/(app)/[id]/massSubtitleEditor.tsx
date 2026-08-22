"use client"

import * as api from "@lib/api.local"

import { MediaEpisodeInfo, MediaInfo, MediaSubtitle } from "@shared/types";
import { ReactNode, useEffect, useState } from "react";

import "./massSubtitleEditor.css"
import { useToast } from "@/app/context/toast";

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const [loading, setLoading] = useState(false);
    const [episodes, setEpisodes] = useState<MediaEpisodeInfo[] | undefined>();

    const { showToast } = useToast();

    useEffect(() => {
        void loadInfo();
    }, [mediaInfo])

    const loadInfo = async () => {
        setLoading(true);

        const eps: MediaEpisodeInfo[] = (await api.library_GetEpisodes(mediaInfo.id, false, false)).sort((a, b) => a.number - b.number);
        setEpisodes(eps);

        setLoading(false);
    }

    const [selectedLanguage, setSelectedLanguage] = useState<string | undefined>(undefined);
    const [inspectingFiles, setInspectingFiles] = useState<File[] | undefined>(undefined);
    const [selectedSubFile, setSelectedSubFile] = useState<(number | undefined)[] | undefined>(undefined);

    const uploadSelectedSubs = () => {
        if (inspectingFiles === undefined || selectedSubFile === undefined || selectedLanguage === undefined)
            return;

        setLoading(true);
        const formData = new FormData();

        for (let i = 0; i < selectedSubFile.length; i++) {
            if ((selectedSubFile[i] ?? -1) == -1 && episodes?.[i].jellyfinId)
                continue;

            formData.append("files", inspectingFiles[selectedSubFile[i]!])
            formData.append("jellyfinIds", episodes![i].jellyfinId!)
        }

        api.media_UploadSubtitles(selectedLanguage, formData)
            .then(() => showToast("Uploaded subtitles", "Success"))
            .catch(() => showToast("Failed to upload subtitles", "Error"))
            .finally(() => setLoading(false));
    }

    const drawUpload = () => {
        const selectSubs = (e: any) => {
            const files: File[] = (Array.from(e.target.files ?? []));
            const sortedFiles: File[] = files.sort((a, b) => a.name.localeCompare(b.name));

            setInspectingFiles(sortedFiles);
            setSelectedSubFile(sortedFiles.map((_, i) => i));
        }

        const changeSelectedSub = (epNumber: number, fileNumber: number) => {
            setSelectedSubFile(prev => {
                if (!prev) return prev;

                const next = [...prev];
                next[epNumber] = fileNumber;
                return next;
            });
        }

        return (
            <>
                <div className="MassSubtitlesEditor_Settings">
                    <select name="language" value={selectedLanguage} onChange={e => setSelectedLanguage(e.target.value)}>
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

                    <input type="file" name="subtitle" multiple accept=".srt,.vtt,.ass,.ssa" onChange={selectSubs} />
                </div>

                <div className="MassSubtitlesEditor_Episodes">
                    <table>
                        <thead>
                            <tr className="MassSubtitlesEditor_Episodes_Header">
                                <th className="MassSubtitlesEditor_Episodes_Header_EpisodeName">Episode</th>
                                <th className="MassSubtitlesEditor_Episodes_Header_File">File</th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                episodes?.map((e, i) => (<tr key={e.number} className="MassSubtitlesEditor_Episodes_Entry">
                                    <td>
                                        Episode. {e.number}
                                    </td>
                                    <td className="MassSubtitlesEditor_Episodes_Entry_Options">
                                        <select value={selectedSubFile?.[i] ?? -1} onChange={e => changeSelectedSub(i, Number.parseInt(e.target.value))}>
                                            <option value={-1}>None</option>
                                            {inspectingFiles?.map((s, i) => <option key={i} value={i}>{s.name}</option>)}
                                        </select>
                                    </td>
                                </tr>))
                            }
                        </tbody>
                    </table>
                </div>

                {
                    loading && (<div className="MassSubtitlesEditor_Loading">
                        Loading...
                    </div>)
                }
            </>
        )
    }

    return (
        <div className="MassSubtitlesEditor">
            <h1>Subtitles</h1>

            {drawUpload()}

            <div className="MassSubtitlesEditor_Controls">
                <div>
                    <button onClick={loadInfo}>Refresh</button>
                    <button onClick={uploadSelectedSubs}>Upload</button>
                </div>
            </div>
        </div>
    )
}