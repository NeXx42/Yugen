export interface User {
    name: string,
    id: string,
}

export interface MediaCardInfo {
    id: string,
    title: string,
}

export interface MediaInfo {
    title: string,
    isDownloaded: boolean,

    seasons: MediaSeason[]
}

export interface MediaSeason {
    episodeNames: string[]
}