"use client"


import * as api from "@lib/api.local"
import { useToast } from "@context/toast";

import { MediaInfo, DownloadRequestInfo, MediaRequest } from "@shared/types";
import { ReactNode, useState } from "react";
import { createPortal } from "react-dom";

import "./seriesControls.css"
import { useModals } from "@/app/context/modalContext";
import LoadingModal from "@/app/modals/loadingModal";
import RenderResult from "next/dist/server/render-result";

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const { showToast } = useToast();
    const { showModal, closeModal } = useModals();

    const [isMenuOpen, setMenuOpen] = useState(false);
    const [bookmarkId, setBookmarkId] = useState(mediaInfo.bookmark);

    const [requestInfo, setRequestInfo] = useState<DownloadRequestInfo | null>(null)
    const [mediaRequest, setMediaRequest] = useState<MediaRequest | null>(null);

    const changeBookmarkId = (val: string) => {
        const newBookmarkId = Number.parseInt(val) ?? 0;

        api.library_UpdateBookmark(mediaInfo.id, newBookmarkId).then(() => {
            setBookmarkId(newBookmarkId);
            showToast("Updated bookmark");
        }).catch(() => showToast("Failed to update", "Error"))
    }

    const requestMedia = async (): Promise<any> => {
        if (mediaRequest?.seriesId == null ||
            mediaRequest?.seasonId == null ||
            mediaRequest?.qualityId == null ||
            mediaRequest?.rootPath == null)
            throw new Error("Invalid argument");

        await api.library_RequestSeries(mediaInfo.id, mediaRequest!);
        openMediaRequestMenu();
    }

    const openMediaRequestMenu = () => {
        api.library_GetSeriesRequest(mediaInfo.id).then(r => {
            setRequestInfo(r);
            setMediaRequest({
                seriesId: r.sonarrRequestId,
                seasonId: r.sonarrSeasonId,

                rootPath: r.selectedRoot != null ? r.roots[r.selectedRoot].path : r.roots[0]?.path,
                qualityId: r.selectedQuality != null ? r.qualities[r.selectedQuality].id : r.qualities[0]?.id,

                monitorSeason: r.monitored
            });
        });
        setMenuOpen(true);
    }

    const resync = async (): Promise<void> => {
        await api.library_SyncMediaDownloads(mediaInfo.id, true);
        await api.library_GetSeriesRequest(mediaInfo.id).then(setRequestInfo);
    }

    const drawMenu = (): ReactNode => {
        if (!isMenuOpen)
            return (<></>);

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

        const toggleSeasonMonitor = () => {
            setMediaRequest(prev => {
                if (prev === null) return prev;
                return {
                    ...prev,
                    monitorSeason: !prev.monitorSeason
                }
            })
        }

        const drawEpisodes = (): ReactNode => {
            if (mediaRequest == null)
                return (<></>)

            if (requestInfo?.downloadedEpisodes == null)
                return (<>Unknown series</>);

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
                    <div>TVDB Id: <strong>{info.sonarrRequestId ?? "UNKNOWN"}</strong></div>
                    <div>Season: <strong>{info.sonarrSeasonId ?? "UNKNOWN"}</strong></div>

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
            </>)
        }

        return createPortal(
            <div className="MediaRequest" onClick={() => setMenuOpen(false)}>
                <div className="MediaRequest_Menu" onClick={e => e.stopPropagation()}>
                    <h1>{mediaInfo.title}</h1>
                    {requestInfo == null ? (<>Failed to load</>) : (drawMenuContent(requestInfo))}
                </div>
            </div >
            , document.body)
    }

    return (
        <>
            <div className="SeriesControls">
                <select className="ViewPage_SeriesControl SeriesControl_Bookmark" value={bookmarkId ?? 0} onChange={e => changeBookmarkId(e.target.value)}>
                    <option value={0}>None</option>
                    <option value={1}>Watching</option>
                    <option value={2}>On Hold</option>
                    <option value={3}>Planning</option>
                    <option value={4}>Completed</option>
                    <option value={5}>Dropped</option>
                </select>
                <button className="ViewPage_SeriesControl" onClick={openMediaRequestMenu}>+</button>
            </div>
            {drawMenu()}
        </>
    )
}