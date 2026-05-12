"use client"

import * as api from "@lib/api.local"
import { useRouter } from "next/navigation";

import "./topbar.css"
import { useState } from "react";

export default function () {
    const [searchQuery, setSearchQuery] = useState<string>("")
    const navigate = useRouter();

    const resync = async () => {
        await api.library_sync();
    }

    const goHome = () => {
        navigate.push("home");
    }

    const search = () => {
        navigate.push(`search?query=${searchQuery}`)
    }

    return (<div className="topbar">
        <button onClick={goHome}>Home</button>
        <button onClick={resync}>Sync</button>

        <input onChange={e => setSearchQuery(e.target.value)} value={searchQuery}></input>
        <button onClick={search}>Search</button>
    </div>)
}