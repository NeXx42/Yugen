"use client"

import { useRouter } from "next/navigation";

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useState } from "react";
import { createPortal } from "react-dom";
import NotificationFoldout from "../foldouts/notificationFoldout";
import SettingsFoldout from "../foldouts/settingsFoldout";
import ProfileFoldout from "../foldouts/profileFoldout";

import "./topbar.css"

type FoldoutType = "None" | "Notifications" | "Profile" | "Settings";

export default function () {
    const [currentFoldout, setCurrentFoldout] = useState<FoldoutType>("None")
    const [notificationCount, setNotificationCount] = useState(0);

    const [searchQuery, setSearchQuery] = useState<string>("")
    const navigate = useRouter();

    const goHome = () => navigate.push("home");
    const search = (inpt: string) => navigate.push(`search?query=${inpt}`)

    useEffect(() => {
        api.notification_Count().then(setNotificationCount);
    }, [])

    const toggleFoldout = (to: FoldoutType) => {
        setCurrentFoldout(to);
    }

    const renderFoldout = (): ReactNode => {
        switch (currentFoldout) {
            case "Notifications": return <NotificationFoldout />
            case "Settings": return <SettingsFoldout requestClose={() => toggleFoldout("None")} />
            case "Profile": return <ProfileFoldout />
        }

        return <></>
    }

    return (<div className="Topbar">
        <div className="Topbar_Left">
            <button className="Topbar_Left_Foldout Topbar_btn" onClick={() => toggleFoldout("Settings")}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 512 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path fill="none" strokeLinecap="round" strokeMiterlimit="10" strokeWidth="48" d="M88 152h336M88 256h336M88 360h336" />
                </svg>
            </button>
            <h1 onClick={goHome}>Yugen</h1>
        </div>
        <div className="Topbar_Centre">
            <input placeholder="Search" value={searchQuery} onChange={e => setSearchQuery(e.target.value)} onKeyDown={(e) => {
                if (e.key === "Enter") {
                    search(e.currentTarget.value);
                }
            }} />
            <button className="Topbar_btn" onClick={() => search(searchQuery)}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 512 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M443.5 420.2L336.7 312.4c20.9-26.2 33.5-59.4 33.5-95.5 0-84.5-68.5-153-153.1-153S64 132.5 64 217s68.5 153 153.1 153c36.6 0 70.1-12.8 96.5-34.2l106.1 107.1c3.2 3.4 7.6 5.1 11.9 5.1 4.1 0 8.2-1.5 11.3-4.5 6.6-6.3 6.8-16.7.6-23.3zm-226.4-83.1c-32.1 0-62.3-12.5-85-35.2-22.7-22.7-35.2-52.9-35.2-84.9 0-32.1 12.5-62.3 35.2-84.9 22.7-22.7 52.9-35.2 85-35.2s62.3 12.5 85 35.2c22.7 22.7 35.2 52.9 35.2 84.9 0 32.1-12.5 62.3-35.2 84.9-22.7 22.7-52.9 35.2-85 35.2z" />
                </svg>
            </button>
        </div>
        <div className="Topbar_Right">
            <button className="Topbar_Right_Notifications Topbar_btn" onClick={() => toggleFoldout("Notifications")}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 576 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M567.938 243.908L462.25 85.374A48.003 48.003 0 0 0 422.311 64H153.689a48 48 0 0 0-39.938 21.374L8.062 243.908A47.994 47.994 0 0 0 0 270.533V400c0 26.51 21.49 48 48 48h480c26.51 0 48-21.49 48-48V270.533a47.994 47.994 0 0 0-8.062-26.625zM162.252 128h251.497l85.333 128H376l-32 64H232l-32-64H76.918l85.334-128z" />
                </svg>

                {notificationCount > 0 && (
                    <a>{notificationCount}</a>
                )}
            </button>

            <button className="Topbar_Right_Profile Topbar_btn" onClick={() => toggleFoldout("Profile")}>
                <img loading="lazy" decoding="async" data-loaded="true" src="https://s4.anilist.co/file/anilistcdn/user/avatar/large/default.png"></img>
            </button>
        </div>


        {
            currentFoldout !== "None" && createPortal((
                <div className="Foldout" onClick={() => toggleFoldout("None")}>
                    {renderFoldout()}
                </div>
            ), document.body)
        }
    </div>)
}