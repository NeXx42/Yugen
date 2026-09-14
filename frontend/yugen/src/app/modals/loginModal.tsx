"use client"

import * as api from "@lib/api.local"
import { useEffect, useState } from "react";

import "./loginModal.css"
import { useRequest } from "../effects/useRequest";

export default function () {
    const { data: jellyfinUsers, error: jellyfinError, loading: jellyfinLoading, execute: jellyfinFetch } = useRequest(api.getAllUsers);

    const [password, setPassword] = useState<string | undefined>(undefined);
    const [username, setUsername] = useState<string>();


    useEffect(() => { jellyfinFetch(); }, []);
    useEffect(() => {
        setUsername(jellyfinUsers?.[0].name)
    }, [jellyfinUsers])

    const login = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (password === undefined || username === undefined)
            return;

        await api.auth_Login(username, password)
        window.location.reload();
    }

    const drawSetup = () => {

    }

    const drawForm = () => {
        return (
            <>
                <h1>Login</h1>

                <form onSubmit={login}>
                    <select value={username} onChange={(e) => setUsername(e.target.value)}>
                        {jellyfinUsers?.map((x, i) => (
                            <option key={i} value={x.name}>{x.name}</option>
                        ))}
                    </select>

                    <input type="password" placeholder="Password" value={password} onChange={x => setPassword(x.target.value)} />
                    <button type="submit">Login</button>
                </form>
            </>
        )
    }

    if (jellyfinError)
        return drawSetup();

    return drawForm();
}