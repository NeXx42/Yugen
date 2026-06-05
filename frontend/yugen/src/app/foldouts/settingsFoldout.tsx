"use client"

import * as api from "@lib/api.local"

import { useRouter, usePathname } from "next/navigation";
import { useEffect, useState } from "react"

import "./settingsFoldout.css"
import SettingsModal from "../modals/settingsModal";

export default function ({ isAuthenticated, requestClose }: { isAuthenticated: boolean, requestClose: () => void }) {
    const [modelOpen, setModalOpen] = useState(false);
    const [links, setLinks] = useState<{ jellyfin: string | null, sonarr: string | null, radarr: string | null }>({ jellyfin: null, sonarr: null, radarr: null });

    useEffect(() => {
        api.settings_Links().then(setLinks);
    }, [])

    const pathname = usePathname();
    const navigate = useRouter();

    const changePage = (path: string) => {
        navigate.push(path);
        requestClose();
    }

    const getNavigationClassName = (needed: string) => {
        return pathname === needed ? "Selected" : "";
    }

    return (<div className="Settings">
        {
            !modelOpen ? (
                <div className="Settings_Foldout" onClick={e => e.stopPropagation()}>
                    <div className="Settings_Foldout_Top">
                        <h1>Yugen</h1>
                        <button onClick={requestClose}>X</button>
                    </div>

                    <div className="Settings_Foldout_Controls">
                        <div className="Settings_Foldout_Navigation">
                            <button className={getNavigationClassName("/home")} onClick={() => changePage("home")}>
                                <svg stroke="currentColor" fill="currentColor" height="24px" width="24px" viewBox="0 0 512 512" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M261.56 101.28a8 8 0 0 0-11.06 0L66.4 277.15a8 8 0 0 0-2.47 5.79L63.9 448a32 32 0 0 0 32 32H192a16 16 0 0 0 16-16V328a8 8 0 0 1 8-8h80a8 8 0 0 1 8 8v136a16 16 0 0 0 16 16h96.06a32 32 0 0 0 32-32V282.94a8 8 0 0 0-2.47-5.79z"></path><path d="m490.91 244.15-74.8-71.56V64a16 16 0 0 0-16-16h-48a16 16 0 0 0-16 16v32l-57.92-55.38C272.77 35.14 264.71 32 256 32c-8.68 0-16.72 3.14-22.14 8.63l-212.7 203.5c-6.22 6-7 15.87-1.34 22.37A16 16 0 0 0 43 267.56L250.5 69.28a8 8 0 0 1 11.06 0l207.52 198.28a16 16 0 0 0 22.59-.44c6.14-6.36 5.63-16.86-.76-22.97z" />
                                </svg>
                                Home
                            </button>
                            {
                                isAuthenticated && (
                                    <button className={getNavigationClassName("/library")} onClick={() => changePage("library")}>
                                        <svg height="24px" viewBox="0 0 24 24" width="24px" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                            <path d="M0 0h24v24H0V0z" fill="none" />
                                            <path d="M0 0h24v24H0V0z" fill="none" />
                                            <path d="M4 6H2v14c0 1.1.9 2 2 2h14v-2H4V6z" />
                                            <path d="M20 2H8c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 10l-2.5-1.5L15 12V4h5v8z" />
                                        </svg>
                                        Library
                                    </button>
                                )
                            }
                            <button className={getNavigationClassName("/schedule")} onClick={() => changePage("schedule")}>
                                <svg stroke="currentColor" fill="currentColor" strokeWidth={0} height="24px" width="24px" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                                    <path fill="none" d="M0 0h24v24H0z" />
                                    <path d="M20 8H4V6h16v2zm-2-6H6v2h12V2zm4 10v8c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2v-8c0-1.1.9-2 2-2h16c1.1 0 2 .9 2 2zm-6 4-6-3.27v6.53L16 16z" />
                                </svg>
                                Schedule
                            </button>
                        </div>

                        <div className="Settings_Foldout_External">
                            {links.jellyfin &&
                                <a href={links.jellyfin} target="_blank" >
                                    <img src="https://jellyfin.org/images/favicon.ico" />
                                    Jellyfin
                                </a>}

                            {links.sonarr &&
                                <a href={links.sonarr} target="_blank" >
                                    <img src="https://sonarr.tv/img/favicon.ico" />
                                    Sonarr
                                </a>}

                            {links.radarr &&
                                <a href={links.radarr} target="_blank" >
                                    <img src="https://radarr.video/img/favicon.ico" />
                                    Radarr
                                </a>}
                        </div>
                    </div>

                    {
                        isAuthenticated && (
                            <div className="Settings_Foldout_Bottom">
                                <button onClick={() => setModalOpen(true)}>Settings</button>
                            </div>
                        )
                    }
                </div>

            ) : (<SettingsModal />)
        }
    </div >
    )
}