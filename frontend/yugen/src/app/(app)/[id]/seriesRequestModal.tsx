"use client"

import * as api from "@lib/api.local"

import { MediaInfo, DownloadRequestInfo, MediaRequest } from "@shared/types";
import { ReactNode, useEffect, useState } from "react";

import "./seriesRequestModal.css"
import LoadingCover from "@/app/components/Misc/LoadingCover";
import ErrorBox from "@/app/components/Misc/errorBox";

export default function ({ mediaInfo, onUpdate, onClose }: { mediaInfo: MediaInfo, onUpdate: () => void, onClose: () => void }) {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | undefined>(undefined);

    const [requestInfo, setRequestInfo] = useState<DownloadRequestInfo | null>(null)
    const [mediaRequest, setMediaRequest] = useState<MediaRequest | null>(null);

    useEffect(() => {
        setLoading(true);
        setError(undefined);

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
        })
            .catch(e => setError(e.message))
            .finally(() => setLoading(false));

    }, [mediaInfo])

    const requestMedia = async (): Promise<any> => {
        if (mediaRequest?.seriesId == null ||
            (requestInfo?.libraryProvider !== 1 && mediaRequest?.seasonId == null) ||
            mediaRequest?.qualityId == null ||
            mediaRequest?.rootPath == null)
            throw new Error("Invalid argument");

        setLoading(true);
        await api.library_RequestSeries(mediaInfo.id, mediaRequest!);
        await resync();
        setLoading(false);

        onUpdate();
    }

    const resync = async (): Promise<void> => {
        setLoading(true);
        await api.library_SyncMediaDownloads(mediaInfo.id, true);
        await api.library_GetSeriesRequest(mediaInfo.id).then(setRequestInfo);
        setLoading(false);
    }

    const research = async (): Promise<void> => {
        setLoading(true);
        await api.library_ResearchMonitored(mediaInfo.id);
        setLoading(false);
    }

    const deleteMedia = async (): Promise<void> => {
        setLoading(true);
        await api.library_DeleteMedia(mediaInfo.id);
        setLoading(false);
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
        if (!requestInfo?.downloadedEpisodes)
            return (<p>Unknown series</p>);

        return (
            <table>
                <thead>
                    <tr className="MediaRequest_Menu_Episodes_Header">
                        <th>Episode</th>
                        <th>Downloaded</th>
                        <th>Monitored</th>
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

    const drawManualLinker = () => {
        const handleSubmit = async (e: any) => {
            e.preventDefault();

            const formData = new FormData(e.target);
            const providerId = Number.parseInt(formData.get("provider")?.toString() ?? "");
            const mediaId = Number.parseInt(formData.get("mediaId")?.toString() ?? "");
            const season = Number.parseInt(formData.get("season")?.toString() ?? "");

            api.library_UploadManualLink(mediaInfo.id, providerId, mediaId, season).then(() => document.location.reload());
        };

        return (<form onSubmit={handleSubmit}>
            <a>Couldn't find link for anime, define manual link</a>
            <div>
                <a>Library Provider</a>
                <select name="provider">
                    <option value="0">Sonarr</option>
                    <option value="1">Radarr</option>
                </select>
            </div>
            <div>
                <a>Media Id</a>
                <input name="mediaId" type="number"></input>
            </div>
            <div>
                <a>Media Season</a>
                <input name="season" type="number"></input>
            </div>

            <button type="submit">Save</button>
        </form>)
    }

    const drawWrapper = () => {
        if (error)
            return <ErrorBox error={""} />

        const drawIds = () => {
            if (!requestInfo)
                return <></>

            if (requestInfo.libraryProvider === 1)
                return <div>TMDB Id: <strong>{requestInfo.sonarrRequestId ?? "UNKNOWN"}</strong></div>;

            return (<>
                <div>TVDB Id: <strong>{requestInfo!.sonarrRequestId ?? "UNKNOWN"}</strong></div>
                <div>Season: <strong>{requestInfo!.sonarrSeasonId ?? "UNKNOWN"}</strong></div>
            </>)
        }

        const drawEpisodesList = () => {
            if (!requestInfo)
                return drawEpisodes();

            if (requestInfo.downloadedEpisodes)
                return (<>
                    <div className="MediaRequest_Menu_Status">
                        <a>Monitored</a>
                        <input type="checkbox" checked={mediaRequest?.monitorSeason ?? false} onChange={e => toggleSeasonMonitor()} />
                    </div>

                    <div className="MediaRequest_Menu_Episodes">
                        {
                            requestInfo?.sonarrRequestId
                                ? drawEpisodes()
                                : drawManualLinker()
                        }
                    </div>

                    <div className="MediaRequest_Menu_Controls">
                        <button onClick={deleteMedia}>Delete</button>

                        <div>
                            <button onClick={research}>Search Monitored</button>
                            <button onClick={requestMedia}>Update Monitor Status</button>
                        </div>
                    </div>
                </>)

            return (<div className="MediaRequest_Menu_DownloadMetadata">
                {
                    requestInfo?.sonarrRequestId != undefined ? (<>
                        <p>Unknown series</p>
                        <button onClick={() => { toggleSeasonMonitor(true); return requestMedia(); }}>Download Provider data</button>
                    </>)
                        : drawManualLinker()
                }
            </div >);
        }

        return (<>
            <div className="MediaRequest_Menu_Ids">
                {drawIds()}
                <div>Root:
                    <select value={requestInfo?.selectedRoot ?? 0} onChange={e => updateSelectedRoot(e.target.value)}>
                        {requestInfo?.roots.map(r => <option key={r.path} value={r.path}>{r.path}</option>) ?? <option>LOADING</option>}
                    </select>
                </div>

                <div>Quality:
                    <select value={requestInfo?.selectedQuality ?? 0} onChange={e => updateSelectedQuality(Number.parseInt(e.target.value))}>
                        {requestInfo?.qualities.map(r => <option key={r.id} value={r.id}>{r.title}</option>) ?? <option>LOADING</option>}
                    </select>
                </div>
                <button onClick={resync}>Refresh</button>
            </div>
            {drawEpisodesList()}
        </>)
    }

    return (
        <div className="MediaRequest_Menu" >
            <h1>{mediaInfo.title}</h1>
            {drawWrapper()}
            {loading && <LoadingCover />}
        </div>)
}