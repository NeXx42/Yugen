"use client"

import * as api from "@lib/api.local"
import { useEffect, useState } from "react";

import "./loginModal.css"
import { useRequest } from "../effects/useRequest";

export default function () {
    const [setupError, setSetupError] = useState<string | undefined>();
    const { data: jellyfinUsers, error: jellyfinError, loading: jellyfinLoading, execute: jellyfinFetch } = useRequest(api.getAllUsers);

    const [password, setPassword] = useState<string | undefined>(undefined);
    const [username, setUsername] = useState<string>();

    const [attemptedSave, setAttemptedSave] = useState(false);
    const [jellyfinUrl, setJellyfinUrl] = useState<string>("");
    const [jellyfinApiKey, setJellyfinApiKey] = useState<string>("");

    useEffect(() => { jellyfinFetch(); }, []);
    useEffect(() => {
        setUsername(jellyfinUsers?.[0].name)
    }, [jellyfinUsers])

    const login = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        console.log(username, password);
        if (password === undefined || username === undefined)
            return;

        await api.auth_Login(username, password)
        window.location.reload();
    }

    const drawSetup = () => {
        const saveJellyfinLinking = async () => {
            await api.setup_Try(jellyfinUrl, jellyfinApiKey).then(() => {
                setAttemptedSave(true);
                jellyfinFetch();
            })
                .catch((e) => {
                    setSetupError(e.message);
                })


        }

        return (<div className="LoginModal_Setup">
            <h1>Setup</h1>

            {
                setupError && <div>
                    {setupError}
                </div>
            }

            <div className="LoginModal_Setup_Group">
                <p>Jellyfin URL</p>
                <input value={jellyfinUrl} onChange={e => setJellyfinUrl(e.target.value)} placeholder=""></input>
            </div>
            <div className="LoginModal_Setup_Group">
                <p>Jellyfin API Key</p>
                <input value={jellyfinApiKey} onChange={e => setJellyfinApiKey(e.target.value)} placeholder=""></input>
            </div>
            <div className="LoginModal_Setup_Controls">
                <button onClick={saveJellyfinLinking}>Save</button>
            </div>
        </div>)
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