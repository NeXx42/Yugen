"use client"

import { MediaCardInfo, SonarrEpisodeInfo, User } from "@shared/types";
import { post, get } from "./api.shared";

export async function auth_Login(username: string, password: string) {
    return (await post<User>("auth/login", { username, password }))!;
}


export async function library_sync() {
    return (await post("catalog/RedownloadLinking"))
}

export async function library_CurrentWatching(): Promise<MediaCardInfo[]> {
    return (await get<MediaCardInfo[]>("library/currentlyWatching"))!;
}

export async function catalog_Upcoming(): Promise<MediaCardInfo[]> {
    return (await get<MediaCardInfo[]>("catalog/Upcoming"))!;
}

export async function catalog_Search(text: string): Promise<MediaCardInfo[]> {
    return (await post<MediaCardInfo[]>("catalog/Search", {
        text: text
    }))!;
}

export async function media_PlayItem(itemId: string): Promise<string> {
    return (await get<string>("media/play"))!
}

export async function library_GetSonarrEpisodes(aniListId: number): Promise<SonarrEpisodeInfo[]> {
    return (await get<SonarrEpisodeInfo[]>(`library/${aniListId}`))!;
}