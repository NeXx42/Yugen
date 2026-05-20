export type BookmarkType = "None" | "Watching" | "OnHold" | "Planning" | "Completed" | "Dropped";

export interface PageResponse<T> {
    page: number,
    pageSize: number,
    totalResults: number,

    data: T[]
}


export interface User {
    name: string,
    id: string,
}

export interface MediaCardInfo {
    aniListId: string,
    title: string,
    type: string,

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

    thumbnailImage: string | null;
    bannerImage: string | null,
    cardImage: string | null,
    colour: string | null,

    bookmark: number | undefined,

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
    hasBeenSeen: boolean;

    url: string | undefined;
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