"use client"

import * as api from "@lib/api.local"

import { useRequest } from "@/app/effects/useRequest";
import { useState } from "react";

export default function () {
    const { error: jellyfinError, execute: jellyfinFetch } = useRequest(api.getAllUsers);

    if (!jellyfinError) {
        window.location.pathname = "home"
    }

    const [setupError, setSetupError] = useState<string | undefined>();

    const [jellyfinUrl, setJellyfinUrl] = useState<string>("");
    const [jellyfinApiKey, setJellyfinApiKey] = useState<string>("");

    const saveJellyfinLinking = async () => {
        await api.setup_Try(jellyfinUrl, jellyfinApiKey).then(() => {
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