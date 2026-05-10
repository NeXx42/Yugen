"use client"

import * as api from "@lib/api.local"

import { User } from "@shared/types";
import router from "next/router";
import { useState } from "react";

interface Props {
    usrs: User[]
}

export default function (props: Props) {
    const [password, setPassword] = useState<string | undefined>(undefined);
    const [username, setUsername] = useState<string | undefined>(undefined);

    const login = async (e: React.FormEvent<HTMLFormElement>) => {
        if (password === undefined || username === undefined)
            return;

        e.preventDefault();
        await api.auth_Login(username, password)

        router.push("/");
    }

    return (
        <form onSubmit={login}>
            <select value={username} onChange={(e) => setUsername(e.target.value)}>
                {props.usrs.map(x => (
                    <option key={x.id} value={x.name}>{x.name}</option>
                ))}
            </select>

            <input type="password" placeholder="Password" value={password} onChange={x => setPassword(x.target.value)} />
            <button type="submit">Login</button>
        </form>
    )
}