"use client"

import { post, get, getPlain, upload, deleteReq } from "./api.shared";
import { ConfigSetting, MediaCardInfo, PageResponse, User, UserNotification, MediaEpisodeInfo, SearchRequest, DownloadRequestInfo, MediaRequest, Playback_Info, MediaInfo } from "@shared/types";

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
export async function settings_Update(): Promise<void> {
    return (await post(`Settings/Update`))!;
}


export async function library_sync(): Promise<number | undefined> {
    return (await post<Promise<number | undefined>>("Library/Sync/Library"));
}
export async function library_SyncWatchHistory() {
    await post(`Library/Sync/History`);
}
export async function library_SyncMediaDownloads(mediaId: number, force: boolean = false) {
    await post(`Library/${mediaId}/SyncDownloads?force=${force}`)
}
export async function library_Search(req: SearchRequest, group: string): Promise<PageResponse<MediaCardInfo>> {
    return (await post<PageResponse<MediaCardInfo>>("library/Search", {
        req,
        group,
    }))!
}
export async function library_Upload(file: FormData) {
    await upload("library/Upload", file);
}
export async function library_UpdateBookmark(mediaId: number, bookmarkId: number) {
    await post(`library/${mediaId}/UpdateBookmark?id=${bookmarkId}`);
}
export async function library_GetEpisodes(mediaId: number, refetch: boolean, clearOld: boolean): Promise<MediaEpisodeInfo[]> {
    return (await get<MediaEpisodeInfo[]>(`library/${mediaId}/Episodes?refetch=${refetch}&clearOld=${clearOld}`))!
}
export async function library_GetFilm(mediaId: number, refetch: boolean): Promise<MediaEpisodeInfo | null> {
    return (await get<MediaEpisodeInfo | null>(`library/${mediaId}/Film?refetch=${refetch}`))!
}
export async function library_CurrentWatching(page: number, pageSize: number): Promise<PageResponse<MediaCardInfo>> {
    return (await get<PageResponse<MediaCardInfo>>(`library/WatchHistory?page=${page}&pageSize=${pageSize}`))!;
}
export async function library_RequestSeries(mediaId: number, mediaRequest: MediaRequest) {
    return (await post(`library/${mediaId}/Request`, mediaRequest))!;
}
export async function library_Request(aniListId: number, rootPath: string, quality: number): Promise<boolean> {
    return (await post(`library/${aniListId}/Request`, {
        rootPath,
        quality
    }))!
}
export async function library_GetSeriesRequest(seriesId: number): Promise<DownloadRequestInfo> {
    return (await get<DownloadRequestInfo>(`library/${seriesId}/Request`))!
}
export async function library_ResearchMonitored(seriesId: number) {
    await post(`library/${seriesId}/ResearchDownloads`)
}
export async function library_DeleteMedia(mediaId: number) {
    await deleteReq(`library/${mediaId}`);
}
export async function library_ClearWatchHistory(seriesId: number) {
    return (await post(`library/${seriesId}/ClearHistory`));
}


export async function catalog_ReloadLinks() {
    await post(`catalog/RedownloadLinking`)
}
export async function catalog_Search(req: SearchRequest): Promise<PageResponse<MediaCardInfo>> {
    return (await post<PageResponse<MediaCardInfo>>("catalog/Search", req))!;
}
export async function catalog_ClearDatabase() {
    await post("catalog/Cache/DatabaseClear");
}
export async function catalog_ClearCache() {
    await post("catalog/Cache/Clear");
}
export async function catalog_Trending(): Promise<MediaInfo[]> {
    return (await get<MediaInfo[]>("catalog/Trending"))!;
}


export async function media_PlaybackInfo(anilistId: number, epNumber: number, itemId: string): Promise<Playback_Info> {
    return (await get<Playback_Info>(`media/${itemId}/PlaybackInfo?anilistId=${anilistId}&episodeNumber=${epNumber}`))!
}
export async function media_UpdateEpisodeTime(mediaId: number, episode: number, runtimeSeconds: number, percentage: number) {
    return (await post(`media/${mediaId}/${episode}/UpdateTime`, {
        runtimeSeconds,
        percentage
    }))!
}
export async function media_UploadSubtitle(jellyfinId: string, language: string, file: FormData) {
    await upload(`media/${jellyfinId}/UploadSubtitle?language=${language}`, file);
}
export async function media_DeleteSubtitle(jellyfinId: string, id: number) {
    await deleteReq(`media/${jellyfinId}/${id}/Subtitle`);
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
export async function notification_MarkAllAsRead() {
    await post(`Notifications/ReadAll`);
}