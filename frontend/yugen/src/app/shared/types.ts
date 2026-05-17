export interface User {
    name: string,
    id: string,
}

export interface MediaCardInfo {
    aniListId: string,
    title: string,

    colour: string,
    cardImg: string,

    nextReleaseDate: number | undefined,

    watchEpisode: number | undefined,
    watchPercentage: number | undefined,
}

export interface MediaInfo {
    id: number,
    title: string,

    bannerImage: string | null,
    cardImage: string | null,
    colour: string | null,

    episodes: MediaEpisodeInfo[]
    connectedMedia: MediaConnection[]
}

export interface MediaEpisodeInfo {
    title: string | null,
    number: number,

    isRecap: boolean,
    isFiller: boolean,

    score: number,
}

export interface MediaConnection {
    season: number | undefined,
    type: string,

    card: MediaCardInfo,
}

export interface SonarrEpisodeInfo {
    episode: number,
    jellyfinId: string,
}

export interface WatchHistory {
    lastWatchedEpisode: number | undefined,
    episodes: WatchHistoryEpisode[],
}

export interface WatchHistoryEpisode {
    episode: number,
    watchPercentage: number | undefined
}



export interface ConfigSetting {
    key: string;
    value: string | undefined;
}