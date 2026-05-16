export interface User {
    name: string,
    id: string,
}

export interface MediaCardInfo {
    aniListId: string,
    title: string,

    colour: string,
    cardImg: string,
}

export interface MediaInfo {
    id: number,
    title: string,

    bannerUrl: string | null,
    cardUrl: string | null,
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