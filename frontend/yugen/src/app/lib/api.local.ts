"use client"

import { post, get, upload } from "./api.shared";
import { ConfigSetting, MediaCardInfo, PageResponse, SonarrEpisodeInfo, User, WatchHistory, UserNotification } from "@shared/types";

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



export async function library_sync(): Promise<number | undefined> {
    return (await post<Promise<number | undefined>>("Library/Sync/Library"));
}
export async function library_SyncWatchHistory() {
    await post(`Library/Sync/History`);
}
export async function library_CurrentWatching(page: number, pageSize: number): Promise<PageResponse<MediaCardInfo>> {
    return (await get<PageResponse<MediaCardInfo>>(`library/WatchHistory?page=${page}&pageSize=${pageSize}`))!;
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
export async function library_UpdateBookmark(mediaId: number, bookmarkId: number) {
    await post(`library/${mediaId}/UpdateBookmark?id=${bookmarkId}`);
}
export async function library_Request(aniListId: number, rootPath: string, quality: number): Promise<boolean> {
    return (await post(`library/${aniListId}/Request`, {
        rootPath,
        quality
    }))!
}



export async function catalog_ReloadLinks() {
    await post(`catalog/RedownloadLinking`)
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
export async function catalog_ClearDatabase() {
    await post("catalog/Cache/DatabaseClear");
}
export async function catalog_ClearCache() {
    await post("catalog/Cache/Clear");
}


export async function media_PlayItem(itemId: string): Promise<string> {
    return (await get<string>("media/play"))!
}



export async function notification_Count(): Promise<number> {
    return (await get<number>("Notifications/Count"))!;
}
export async function notification_Get(): Promise<UserNotification[]> {
    return (await get<UserNotification[]>("Notifications"))!;
}
export async function notification_Read(id: number) {
    await post(`Notifications/${id}/Read`);
}
export async function notification_ClearRead() {
    await post(`Notifications/Clear`);
}