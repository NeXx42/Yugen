"use server"

import { get } from "./api.shared";

import { User } from "@shared/types";

export async function getAllUsers(): Promise<User[]> {
    return (await get("Auth/all", {
        revalidate: 10
    }))!;
}