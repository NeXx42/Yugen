"use client"


import * as api from "@lib/api.local"
import { useToast } from "@context/toast";

import { MediaInfo, DownloadRequestInfo, MediaRequest } from "@shared/types";
import { ReactNode, useEffect, useState } from "react";
import { createPortal } from "react-dom";

import { useModals } from "@/app/context/modalContext";
import LoadingModal from "@/app/modals/loadingModal";

import "./seriesRequestModal.css"


export default function ({ mediaInfo, onUpdate, onClose }: { mediaInfo: MediaInfo, onUpdate: () => void, onClose: () => void }) {
    const { showModal, closeModal } = useModals();

    const [requestInfo, setRequestInfo] = useState<DownloadRequestInfo | null>(null)
    const [mediaRequest, setMediaRequest] = useState<MediaRequest | null>(null);

    useEffect(() => {
        api.library_GetSeriesRequest(mediaInfo.id).then(r => {
            setRequestInfo(r);
            setMediaRequest({
                seriesId: r.sonarrRequestId,
                seasonId: r.sonarrSeasonId,
                libraryProvider: r.libraryProvider,

                rootPath: r.selectedRoot != null ? r.roots[r.selectedRoot].path : r.roots[0]?.path,
                qualityId: r.selectedQuality != null ? r.qualities[r.selectedQuality].id : r.qualities[0]?.id,

                monitorSeason: r.monitored
            });
        });
    }, [mediaInfo])

    const requestMedia = async (): Promise<any> => {
        if (mediaRequest?.seriesId == null ||
            (requestInfo?.libraryProvider !== 1 && mediaRequest?.seasonId == null) ||
            mediaRequest?.qualityId == null ||
            mediaRequest?.rootPath == null)
            throw new Error("Invalid argument");

        await api.library_RequestSeries(mediaInfo.id, mediaRequest!);
        await resync();

        onUpdate();
    }

    const resync = async (): Promise<void> => {
        await api.library_SyncMediaDownloads(mediaInfo.id, true);
        await api.library_GetSeriesRequest(mediaInfo.id).then(setRequestInfo);
    }

    const updateSelectedQuality = (val: number) => {
        if (val == Number.NaN)
            return;

        setMediaRequest(prev => {
            if (prev === null) return prev;

            return {
                ...prev,
                qualityId: val
            }
        })
    }

    const updateSelectedRoot = (val: string) => {
        setMediaRequest(prev => {
            if (prev === null) return prev;

            return {
                ...prev,
                rootPath: val
            }
        })
    }

    const toggleSeasonMonitor = (to?: boolean) => {
        setMediaRequest(prev => {
            if (prev === null) return prev;
            return {
                ...prev,
                monitorSeason: to ?? !prev.monitorSeason
            }
        })
    }

    const drawEpisodes = (): ReactNode => {
        if (mediaRequest == null)
            return (<></>)

        if (requestInfo?.downloadedEpisodes == null)
            return (<p>Unknown series</p>);

        return (
            <table>
                <thead>
                    <tr className="MediaRequest_Menu_Episodes_Header">
                        <th>Episode</th>
                        <th>Downloaded</th>
                        <th> Monitored</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        requestInfo.downloadedEpisodes.sort((a, b) => a.episodeNumber - b.episodeNumber).map((e, i) => (<tr key={e.episodeNumber} className="MediaRequest_Menu_Episodes_Entry">
                            <td>
                                Episode. {e.episodeNumber}
                            </td>
                            <td>
                                <input type="checkbox" checked={e.jellyfinId != null} readOnly={true} />
                            </td>
                            <td>
                                <input type="checkbox" checked={requestInfo.downloadedEpisodes![i].monitored} readOnly={true} />
                            </td>
                        </tr>))
                    }
                </tbody>
            </table>
        )
    }

    const drawMenuContent = (info: DownloadRequestInfo): ReactNode => {

        return (<>
            <div className="MediaRequest_Menu_Ids">
                {
                    info.libraryProvider === 1 ?
                        (
                            <div>TMDB Id: <strong>{info.sonarrRequestId ?? "UNKNOWN"}</strong></div>
                        )
                        : (
                            <>
                                <div>TVDB Id: <strong>{info.sonarrRequestId ?? "UNKNOWN"}</strong></div>
                                <div>Season: <strong>{info.sonarrSeasonId ?? "UNKNOWN"}</strong></div>
                            </>
                        )
                }

                <div>Root:
                    <select value={requestInfo!.selectedRoot ?? 0} onChange={e => updateSelectedRoot(e.target.value)}>
                        {info.roots.map(r => <option key={r.path} value={r.path}>{r.path}</option>)}
                    </select>
                </div>

                <div>Quality:
                    <select value={requestInfo!.selectedQuality ?? 0} onChange={e => updateSelectedQuality(Number.parseInt(e.target.value))}>
                        {info.qualities.map(r => <option key={r.id} value={r.id}>{r.title}</option>)}
                    </select>
                </div>

                <button onClick={() => showModal(<LoadingModal loadingCall={() => resync()} closeRequest={closeModal} />)}>Refresh</button>
            </div>

            {
                requestInfo?.downloadedEpisodes ? (
                    <>
                        <div className="MediaRequest_Menu_Status">
                            <a>Monitored</a>
                            <input type="checkbox" checked={mediaRequest!.monitorSeason} onChange={e => toggleSeasonMonitor()} />
                        </div>

                        <div className="MediaRequest_Menu_Episodes">
                            {drawEpisodes()}

                        </div>

                        <div className="MediaRequest_Menu_Controls">
                            <button onClick={() => showModal(<LoadingModal loadingCall={() => api.library_DeleteMedia(mediaInfo.id)} closeRequest={closeModal} />)}>Delete</button>

                            <div>
                                <button onClick={() => showModal(<LoadingModal loadingCall={() => api.library_ResearchMonitored(mediaInfo.id)} closeRequest={closeModal} />)}>Search Monitored</button>
                                <button onClick={() => showModal(<LoadingModal loadingCall={() => requestMedia()} closeRequest={closeModal} />)}>Update Monitor Status</button>
                            </div>
                        </div>
                    </>
                ) : (
                    <div className="MediaRequest_Menu_DownloadMetadata">
                        <p>Unknown series</p>
                        <button onClick={() => showModal(<LoadingModal loadingCall={() => { toggleSeasonMonitor(true); return requestMedia(); }} closeRequest={closeModal} />)}>Download Provider data</button>
                    </div>
                )
            }


        </>)
    }

    return createPortal(
        <div className="MediaRequest" onClick={() => onClose()}>
            <div className="MediaRequest_Menu" onClick={e => e.stopPropagation()}>
                <h1>{mediaInfo.title}</h1>
                {requestInfo == null ? (<>Failed to load</>) : (drawMenuContent(requestInfo))}
            </div>
        </div >
        , document.body)
}