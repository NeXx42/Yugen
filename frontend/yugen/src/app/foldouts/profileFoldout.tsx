"use client"

import * as api from "@lib/api.local"

import "./profileFoldout.css"

import { RefObject, useEffect, useState } from "react";
import { useRequest } from "../effects/useRequest";

import LoadingCover from "../components/Misc/LoadingCover";
import { useUser } from "../context/userContext";

export default function ({ ref }: { ref: RefObject<HTMLDivElement | null> }) {
    const { session, login: loginFunc, logout: logoutFunc } = useUser();

    if (!session) {
        const { data: jellyfinUsers, error: jellyfinError, execute: jellyfinFetch } = useRequest(api.getAllUsers);

        if (jellyfinError) {
            window.location.pathname = "setup";
            return;
        }

        const [password, setPassword] = useState<string | undefined>(undefined);
        const [username, setUsername] = useState<string>();

        const [failedLogin, setFailedLogin] = useState<boolean>(false);
        const [loading, setLoading] = useState<boolean>(false);

        useEffect(() => { jellyfinFetch(); }, []);
        useEffect(() => {
            setUsername(jellyfinUsers?.[0]?.name)
        }, [jellyfinUsers])

        const login = () => {
            if (loading)
                return;

            if (password === undefined || username === undefined)
                return;

            setLoading(true);
            setFailedLogin(false);

            loginFunc(username, password)
                .catch(_ => setFailedLogin(true))
                .then(res => {
                    if (res)
                        window.location.reload()
                    else
                        setFailedLogin(true);
                })
                .finally(() => setLoading(false));
        }

        const checkForPasswordSubmit = (e: React.KeyboardEvent<HTMLInputElement>) => {
            if (e.key === "Enter") {
                login();
            }
        }

        return (
            <div className="Profile" onClick={e => e.stopPropagation()} style={{ width: "250px" }} ref={ref}>
                <select style={{ gridColumn: "span 2" }} value={username}>
                    {jellyfinUsers?.map(usr => <option key={usr.id} value={usr.name}>{usr.name}</option>)}
                </select>
                <input style={{ gridColumn: "span 2" }} type="password" placeholder="Password" value={password} onChange={p => setPassword(p.target.value)} onKeyDown={checkForPasswordSubmit} />

                {failedLogin && <span style={{ gridColumn: "1" }}>Invalid password</span>}
                <button style={{ gridColumn: "2" }} onClick={login}>Login</button>

                {loading && <LoadingCover />}
            </div >
        )
    }

    const logout = () => {
        logoutFunc().finally(() => {
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