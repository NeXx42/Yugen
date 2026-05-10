"use client"

import { User } from "@shared/types";
import { post, get } from "./api.shared";

export async function auth_Login(username: string, password: string) {
    return (await post<User>("auth/login", { username, password }))!;
}