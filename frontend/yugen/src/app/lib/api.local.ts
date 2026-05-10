"use client"

import { MediaCard, User } from "@shared/types";
import { post, get } from "./api.shared";

export async function auth_Login(username: string, password: string) {
    return (await post<User>("auth/login", { username, password }))!;
}


export async function library_sync() {
    return (await post("library/sync"))
}

export async function library_CurrentWatching(): Promise<MediaCard[]> {
    return (await get<MediaCard[]>("library/currentlyWatching"))!;
}