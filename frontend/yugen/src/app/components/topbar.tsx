"use client"

import * as api from "@lib/api.local"
import { useRouter } from "next/navigation";

import "./topbar.css"

export default function () {
    const navigate = useRouter();

    const resync = async () => {
        await api.library_sync();
    }

    const goHome = () => {
        navigate.push("home");
    }

    return (<div className="topbar">
        <button onClick={goHome}>Home</button>
        <button onClick={resync}>Sync</button>
    </div>)
}