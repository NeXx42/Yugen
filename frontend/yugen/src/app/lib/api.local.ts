"use client"

import { MediaCardInfo, User } from "@shared/types";
import { post, get } from "./api.shared";

export async function auth_Login(username: string, password: string) {
    return (await post<User>("auth/login", { username, password }))!;
}


export async function library_sync() {
    return (await post("library/sync"))
}

export async function library_CurrentWatching(): Promise<MediaCardInfo[]> {
    return (await get<MediaCardInfo[]>("library/currentlyWatching"))!;
}

export async function media_PlayItem(itemId: string): Promise<string> {
    return (await get<string>("media/play"))!
}