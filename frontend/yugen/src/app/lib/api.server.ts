"use server"

import * as api from "./api.shared";
import { cookies } from "next/headers";

import { MediaInfo, User } from "@shared/types";

async function postWithAuth<T>(uri: string, obj?: any, nextCaching: NextFetchRequestConfig | undefined = undefined): Promise<T | undefined> {
    const cookieStore = cookies();
    const authToken: string | undefined = (await cookieStore).get("AuthToken")?.value;

    return await api.post<T>(uri, obj, nextCaching, authToken)
}

async function getWithAuth<T>(uri: string, nextCaching: NextFetchRequestConfig | undefined = undefined): Promise<T | undefined> {
    const cookieStore = cookies();
    const authToken: string | undefined = (await cookieStore).get("AuthToken")?.value;

    return await api.get<T>(uri, nextCaching, authToken)
}


export async function getAllUsers(): Promise<User[]> {
    return (await api.get("Auth/all", {
        revalidate: 10
    }))!;
}

export async function catalog_GetInfo(aniListId: number): Promise<MediaInfo> {
    return (await getWithAuth<MediaInfo>(`catalog/${aniListId}`, {
        revalidate: 60
    }))!;
}



export async function media_SyncWatchTime(aniListId: number): Promise<void> {
    return (await postWithAuth(`media/${aniListId}/SyncWatchHistory`, {
        revalidate: 60
    }))!;
}