"use server"

import * as api from "./api.shared";
import { cookies } from "next/headers";

import { MediaCardInfo, MediaInfo, PageResponse, SearchCriteria, User, CaughtResponse } from "@shared/types";

async function postWithAuth<T>(uri: string, obj?: any, nextCaching: NextFetchRequestConfig | undefined = undefined): Promise<CaughtResponse<T>> {
    const cookieStore = cookies();
    const authToken: string | undefined = (await cookieStore).get("AuthToken")?.value;

    try {
        const res: T = (await api.post<T>(uri, obj, nextCaching, authToken))!;
        return {
            data: res,
            error: null
        }
    }
    catch {
        return {
            data: null,
            error: "test"
        }
    }
}

async function getWithAuth<T>(uri: string, nextCaching: NextFetchRequestConfig | undefined = undefined): Promise<CaughtResponse<T>> {
    const cookieStore = cookies();
    const authToken: string | undefined = (await cookieStore).get("AuthToken")?.value;

    try {
        const res: T = (await api.get<T>(uri, nextCaching, authToken))!;
        return {
            data: res,
            error: null
        }
    }
    catch {
        return {
            data: null,
            error: "test"
        }
    }
}


export async function getAllUsers(): Promise<User[]> {
    return (await api.get("Auth/all", {
        revalidate: 10
    }))!;
}

export async function catalog_GetInfo(aniListId: number): Promise<CaughtResponse<MediaInfo>> {
    return await getWithAuth<MediaInfo>(`catalog/${aniListId}`, {
        revalidate: 60
    });
}
export async function library_CurrentWatching(page: number, pageSize: number): Promise<CaughtResponse<PageResponse<MediaCardInfo>>> {
    return await getWithAuth<PageResponse<MediaCardInfo>>(`library/WatchHistory?page=${page}&pageSize=${pageSize}`);
}

export async function catalog_Upcoming(): Promise<CaughtResponse<MediaCardInfo[]>> {
    return await getWithAuth<MediaCardInfo[]>("catalog/Upcoming");
}
export async function catalog_Trending(): Promise<CaughtResponse<MediaInfo[]>> {
    return await getWithAuth<MediaInfo[]>("catalog/Trending");
}
export async function catalog_SearchCriteria(): Promise<CaughtResponse<SearchCriteria>> {
    return await getWithAuth<SearchCriteria>("catalog/SearchCriteria");
}


export async function media_SyncWatchTime(aniListId: number): Promise<CaughtResponse<void>> {
    return await postWithAuth(`media/${aniListId}/SyncWatchHistory`, {
        revalidate: 60
    });
}