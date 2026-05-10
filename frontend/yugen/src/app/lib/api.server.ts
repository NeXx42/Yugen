"use server"

import * as api from "./api.shared";
import { cookies } from "next/headers";

import { MediaCard, User } from "@shared/types";

async function postWithAuth<T>(uri: string, obj?: any, nextCaching: NextFetchRequestConfig | undefined = undefined): Promise<T | undefined> {
    const cookieStore = cookies();
    const authToken: string | undefined = (await cookieStore).get("AuthToken")?.value;

    return await api.post<T>(uri, obj, nextCaching)
}

async function getWithAuth<T>(uri: string, nextCaching: NextFetchRequestConfig | undefined = undefined): Promise<T | undefined> {
    const cookieStore = cookies();
    const authToken: string | undefined = (await cookieStore).get("AuthToken")?.value;

    return await api.get<T>(uri, nextCaching)
}


export async function getAllUsers(): Promise<User[]> {
    return (await api.get("Auth/all", {
        revalidate: 10
    }))!;
}