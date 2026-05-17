"use client"

import { ReactNode, useEffect, useState } from "react";

import * as api from "@lib/api.local"

import { MediaEpisodeInfo, MediaInfo, SonarrEpisodeInfo, WatchHistory, WatchHistoryEpisode } from "@shared/types";
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";

import "./mediaContainer.css"

export interface EpisodeInfo {
    episode: MediaEpisodeInfo;

    watchData: WatchHistoryEpisode | undefined,
    downloadedData: SonarrEpisodeInfo | undefined;
}

export default function ({ mediaInfo }: { mediaInfo: MediaInfo }) {
    const [episodeInfo, setEpisodeInfo] = useState<EpisodeInfo[]>()
    const [selectedEpisode, setSelectedEpisode] = useState<number | undefined>();

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

    console.log(selectedEpisode);

    return (
        <div className="MediaContainer">
            <div className="MediaContainer_Media">
                <MediaPlayer itemId={selectedEpisode !== undefined ? episodeInfo?.find(e => e.episode.number == selectedEpisode)?.downloadedData?.jellyfinId : undefined} />
                <EpisodeList selectedItem={selectedEpisode} setSelectedItem={setSelectedEpisode} episodes={episodeInfo} />
            </div>

            <div className="MediaContainer_EpisodeInfo ViewPageContainer">
                <div className="MediaContainer_EpisodeInfo_Left">
                    {drawEpisodeInfo()}
                </div>
            </div>
        </div>
    )
}