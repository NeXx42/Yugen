import { exportPages } from "next/dist/export/worker";

export type BookmarkType = "None" | "Watching" | "OnHold" | "Planning" | "Completed" | "Dropped";

export interface PageResponse<T> {
    page: number,
    pageSize: number,
    totalResults: number,

    data: T[]
}

export interface CaughtResponse<T> {
    data: T | null,
    error: string | null
}



export interface User {
    name: string,
    id: string,
}

export interface MediaCardInfo {
    aniListId: number,
    title: string,
    type: string | null,

    releasing: boolean,
    year: number | null,

    colour: string,
    cardImg: string,
    bannerImage: string | null,

    nextReleaseDate: number | undefined,

    watchEpisode: number | undefined,
    watchLastTime: number | undefined,
    watchPercentage: number | undefined,
}

export interface MediaInfo {
    id: number,
    title: string,
    description: string | null;
    type: string | null,

    status: string | null,
    startDate: number | null,
    endDate: number | null,
    episodeCount: number | null,
    duration: number | null,
    season: string | null,

    thumbnailImage: string | null;
    bannerImage: string | null,
    cardImage: string | null,
    colour: string | null,

    bookmark: number | undefined,

    tags: MediaTag[],
    connectedMedia: MediaConnection[]
}

export interface MediaEpisodeInfo {
    title: string | null,
    thumbnail: string | null,
    number: number,

    isRecap: boolean,
    isFiller: boolean,

    score: number,

    jellyfinId: string | null

    watchDate: number | null,
    watchPercentage: number | null,
}

export interface MediaTag {
    id: number,
    title: string
}

export interface MediaConnection {
    season: number | undefined,
    type: string,

    card: MediaCardInfo,
}

export interface WatchHistoryEpisode {
    episode: number,
    watchPercentage: number | undefined
}



export interface ConfigSetting {
    key: string;
    value: string | undefined;
}


export interface UserNotification {
    id: number;
    time: number;
    eventName: string;

    title: string | undefined;
    reason: string | undefined;
    icon: string | undefined,
    bannerIcon: string | undefined,
    url: string | undefined;

    hasBeenSeen: boolean;
}


export interface SearchCriteria {
    genres: string[],
    tags: SearchCriteria_LookupPair[],
}

export interface SearchCriteria_LookupPair {
    id: number,
    name: string
}

export interface SearchRequest {
    page: number,
    pageSize: number,

    text: string | null,
    sort: number | null,
}


// imports

export interface MediaRequest {
    seriesId: number | null,
    seasonId: number | null,

    qualityId: number | null,
    rootPath: string | null,

    monitorSeason: boolean,
}

export interface DownloadRequestInfo {
    monitored: boolean

    sonarrRequestId: number | null,
    sonarrSeasonId: number | null,

    selectedRoot: number | null,
    selectedQuality: number | null,

    roots: DownloadRequestInfo_Root[]
    qualities: DownloadRequestInfo_Quality[]

    downloadedEpisodes: DownloadRequestInfo_Episode[] | null
}

export interface DownloadRequestInfo_Episode {
    providerId: number,
    episodeNumber: number,
    monitored: boolean,

    jellyfinId: string | null,
}

export interface DownloadRequestInfo_Root {
    path: string
    freeSpace: number | null,
}

export interface DownloadRequestInfo_Quality {
    id: number,
    title: string
}

// playback

export interface Playback_Info {
    jellyfinId: string,

    sources: Playback_Info_Source[]
}

export interface Playback_Info_Source {
    id: string
    subs: Playback_Info_Subtitle[]
}

export interface Playback_Info_Subtitle {
    language: string,
    uri: string
}