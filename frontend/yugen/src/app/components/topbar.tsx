"use client"

import * as api from "@lib/api.local"

import "./topbar.css"

export default function () {

    const resync = async () => {
        await api.library_sync();
    }

    return (<div className="topbar">
        <a>YO</a>
        <button onClick={resync}>Sync</button>
    </div>)
}