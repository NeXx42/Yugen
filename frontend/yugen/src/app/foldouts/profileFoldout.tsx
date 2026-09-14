"use client"

import * as api from "@lib/api.local"

import "./profileFoldout.css"

import { RefObject, useEffect, useState } from "react";
import { useRequest } from "../effects/useRequest";

export default function ({ isAuthenticated, ref }: { isAuthenticated: boolean, ref: RefObject<HTMLDivElement | null> }) {
    if (!isAuthenticated) {
        const { data: jellyfinUsers, error: jellyfinError, loading: jellyfinLoading, execute: jellyfinFetch } = useRequest(api.getAllUsers);

        if (jellyfinError) {
            window.location.pathname = "setup";
            return;
        }

        const [password, setPassword] = useState<string | undefined>(undefined);
        const [username, setUsername] = useState<string>();

        useEffect(() => { jellyfinFetch(); }, []);
        useEffect(() => {
            setUsername(jellyfinUsers?.[0]?.name)
        }, [jellyfinUsers])

        const login = async () => {
            if (password === undefined || username === undefined)
                return;

            await api.auth_Login(username, password)
            window.location.reload();
        }

        return (<div className="Profile" onClick={e => e.stopPropagation()} style={{ width: "250px" }} ref={ref}>
            <select style={{ gridColumn: "span 2" }} value={username}>
                {jellyfinUsers?.map(usr => <option key={usr.id} value={usr.name}>{usr.name}</option>)}
            </select>
            <input style={{ gridColumn: "span 2" }} type="password" placeholder="Password" value={password} onChange={p => setPassword(p.target.value)} onSubmit={login} />
            <button style={{ gridColumn: "2" }} onClick={login}>Login</button>
        </div >)
    }

    const logout = () => {
        api.auth_Logout().finally(() => {
            window.location.reload()
        });
    }

    return (<div className="Profile" onClick={e => e.stopPropagation()} style={{ width: "150px" }} ref={ref}>
        <a href="https://anilist.co/home" target="_blank">MAL</a>
        <a href="https://myanimelist.net/" target="_blank">Anilist</a>
        <div style={{ gridColumn: "span 2" }}>
            <button onClick={logout}>Logout</button>
        </div>
    </div >)
}