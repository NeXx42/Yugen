"use client"

import { ConfigSetting, MediaCardInfo, PageResponse, SonarrEpisodeInfo, User, WatchHistory } from "@shared/types";
import { post, get, upload } from "./api.shared";

export async function auth_Login(username: string, password: string) {
    return (await post<User>("auth/login", { username, password }))!;
}

export async function auth_Logout() {
    await post("auth/logout");
}


export async function settings_Load() {
    return (await get<ConfigSetting[]>("Settings"))!;
}

export async function settings_Save(key: string, value: string | undefined) {
    return (await post(`Settings/${key}`, { value: value }))!;
}

export async function library_sync() {
    await post("Library/Sync/Library");
}

export async function library_SyncWatchHistory() {
    await post(`Library/Sync/History`);
}


export async function catalog_ReloadLinks() {
    await post(`catalog/RedownloadLinking`)
}

export async function library_CurrentWatching(page: number, pageSize: number): Promise<PageResponse<MediaCardInfo>> {
    return (await get<PageResponse<MediaCardInfo>>(`library/WatchHistory?page=${page}&pageSize=${pageSize}`))!;
}

export async function catalog_Upcoming(): Promise<MediaCardInfo[]> {
    return (await get<MediaCardInfo[]>("catalog/Upcoming"))!;
}

export async function catalog_Search(text: string, page: number, pageSize: number): Promise<PageResponse<MediaCardInfo>> {
    return (await post<PageResponse<MediaCardInfo>>("catalog/Search", {
        text: text,
        page,
        pageSize
    }))!;
}

export async function library_GetSonarrEpisodes(aniListId: number): Promise<SonarrEpisodeInfo[]> {
    return (await get<SonarrEpisodeInfo[]>(`library/${aniListId}`))!;
}

export async function library_GetWatchHistoryForSeries(aniListId: number): Promise<WatchHistory> {
    return (await get<WatchHistory>(`library/${aniListId}/WatchHistory`))!;
}

export async function library_Search(page: number, pageSize: number, group: string): Promise<PageResponse<MediaCardInfo>> {
    return (await post<PageResponse<MediaCardInfo>>("library/Search", {
        page,
        pageSize,
        group,
    }))!
}

export async function library_Upload(file: FormData) {
    await upload("library/Upload", file);
}



export async function media_PlayItem(itemId: string): Promise<string> {
    return (await get<string>("media/play"))!
}

