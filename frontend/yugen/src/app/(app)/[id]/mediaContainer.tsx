"use client"

import { ReactNode, useEffect, useState } from "react";

import * as api from "@lib/api.local"

import { MediaEpisodeInfo, MediaInfo, SonarrEpisodeInfo, WatchHistory, WatchHistoryEpisode } from "@shared/types";
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";

import "./mediaContainer.css"
import { useToast } from "@context/toast";

export interface EpisodeInfo {
    episode: MediaEpisodeInfo;

    watchData: WatchHistoryEpisode | undefined,
    downloadedData: SonarrEpisodeInfo | undefined;
}

export default function ({ mediaInfo, upcomingEpisode }: { mediaInfo: MediaInfo, upcomingEpisode: number | null }) {
    const { showToast } = useToast();

    const [episodeInfo, setEpisodeInfo] = useState<EpisodeInfo[]>()
    const [selectedEpisode, setSelectedEpisode] = useState<number | undefined>();

    const [bookmarkId, setBookmarkId] = useState(mediaInfo.bookmark);

    useEffect(() => {
        const loadExtraData = async () => {
            const [downloadData, watchData] = await Promise.all([
                api.library_GetSonarrEpisodes(mediaInfo.id),
                api.library_GetWatchHistoryForSeries(mediaInfo.id)
            ]);

            setEpisodeInfo(mediaInfo.episodes.map((ep) => {
                const downloadedData: SonarrEpisodeInfo | undefined = downloadData.find(d => d.episode === ep.number) ?? undefined;
                const episodeWatchData: WatchHistoryEpisode | undefined = watchData?.episodes.find(w => w.episode === ep.number) ?? undefined;

                return {
                    episode: ep,
                    downloadedData,
                    watchData: episodeWatchData
                }
            }))

            setSelectedEpisode(watchData?.lastWatchedEpisode ?? mediaInfo.episodes[0].number)
        }

        loadExtraData();
    }, [mediaInfo])

    const drawEpisodeInfo = (): ReactNode => {
        const ep = selectedEpisode !== undefined ? episodeInfo?.find(e => e.episode.number === selectedEpisode) : undefined;

        if (ep == undefined)
            return (<></>);

        return (<>
            <h2>{`${selectedEpisode}. ${ep.episode.title}`}</h2>
            <a>{ep.episode.score}</a>
        </>)
    }

    const changeBookmarkId = (val: string) => {
        const newBookmarkId = Number.parseInt(val) ?? 0;

        api.library_UpdateBookmark(mediaInfo.id, newBookmarkId).then(() => {
            setBookmarkId(newBookmarkId);
            showToast("Updated bookmark");
        }).catch(() => showToast("Failed to update", "Error"))
    }

    const drawBookmark = (): ReactNode => {

        return (
            <select value={bookmarkId ?? 0} onChange={e => changeBookmarkId(e.target.value)}>
                <option value={0}>None</option>
                <option value={1}>Watching</option>
                <option value={2}>On Hold</option>
                <option value={3}>Planning</option>
                <option value={4}>Completed</option>
                <option value={5}>Dropped</option>
            </select>
        )
    }

    return (
        <div className="MediaContainer">
            <div className="MediaContainer_Media">
                <MediaPlayer mediaInfo={mediaInfo} episode={selectedEpisode !== undefined ? episodeInfo?.find(e => e.episode.number == selectedEpisode) : undefined} bookmarkNode={drawBookmark()} />
                <EpisodeList selectedItem={selectedEpisode} setSelectedItem={setSelectedEpisode} episodes={episodeInfo} upcomingEpisode={upcomingEpisode} />
            </div>

            <div className="MediaContainer_EpisodeInfo ViewPageContainer">
                <div className="MediaContainer_EpisodeInfo_Left">
                    {drawEpisodeInfo()}
                </div>
            </div>
        </div>
    )
}