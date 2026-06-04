import { exportPages } from "next/dist/export/worker";

export type BookmarkType = "None" | "Watching" | "OnHold" | "Planning" | "Completed" | "Dropped";

export type Season = "WINTER" | "SPRING" | "SUMMER" | "FALL";
export const seasonLookup: Season[] = ["WINTER", "SPRING", "SUMMER", "FALL"]

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

    status: string | null,
    nextReleaseDate: number | undefined,

    year: number | null,
    season: Season | null,

    colour: string,
    cardImg: string,
    banner: string | null,


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
    upcomingEpisode: number | null,

    thumbnailImage: string | null;
    bannerImage: string | null,
    cardImage: string | null,
    colour: string | null,

    bookmark: number | undefined,

    genres: string[],
    tags: MediaTag[],
    recommended: MediaCardInfo[]
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

    text?: string,
    sort?: number,

    lesserStartDate?: number,
    season?: Season,
    year?: number,
    format?: string,
    status?: string,
}


// imports

export interface MediaRequest {
    seriesId: number | null,
    seasonId: number | null,
    libraryProvider: number

    qualityId: number | null,
    rootPath: string | null,

    monitorSeason: boolean,
}

export interface DownloadRequestInfo {
    monitored: boolean

    sonarrRequestId: number | null,
    sonarrSeasonId: number | null,
    libraryProvider: number

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
    historicalTicks: number | null,
    jellyfinId: string,

    segments: PlaybackInfo_Segment[]
    sources: Playback_Info_Source[]
}

export interface PlaybackInfo_Segment {
    start: number,
    duration: number,
}

export interface Playback_Info_Source {
    id: string,
    subs: Playback_Info_Subtitle[],
    audio: Playback_Info_AudioSource[]
}

export interface Playback_Info_Subtitle {
    id: number,
    title: string | undefined
    language: string | undefined,
    uri: string
    isExternal: boolean,
}

export interface Playback_Info_AudioSource {
    id: number,
    title: string | undefined,
    isDefault: boolean
}