"use client"

import { useRouter, usePathname } from "next/navigation";
import { useState } from "react"

import "./settingsFoldout.css"
import SettingsModal from "../modals/settingsModal";

export default function ({ requestClose }: { requestClose: () => void }) {
    const [modelOpen, setModalOpen] = useState(false);

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

                    <div className="Settings_Foldout_Navigation">
                        <button className={getNavigationClassName("/home")} onClick={() => changePage("home")}>Home</button>
                        <button className={getNavigationClassName("/library")} onClick={() => changePage("library")}>Library</button>
                        <button className={getNavigationClassName("/history")} onClick={() => changePage("history")}>Watch History</button>
                        <button className={getNavigationClassName("/schedule")} onClick={() => changePage("schedule")}>Schedule</button>
                    </div>

                    <div className="Settings_Foldout_Bottom">
                        <button onClick={() => setModalOpen(true)}>Settings</button>
                    </div>
                </div>

            ) : (<SettingsModal />)
        }
    </div >
    )
}