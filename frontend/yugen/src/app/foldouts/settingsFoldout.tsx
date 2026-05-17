
import * as api from "@lib/api.local"
import { ReactNode, useEffect, useState } from "react"

import "./settingsFoldout.css"

type SettingsGroup = "Jellyfin" | "Sonarr" | "Providers";

export default function () {
    const [modelOpen, setModalOpen] = useState(false);

    const [selectedSettingsGroup, setSelectedSettingsGroup] = useState<SettingsGroup>("Jellyfin");
    const groups: SettingsGroup[] = ["Jellyfin", "Sonarr", "Providers"];

    const [savedConfigValues, setSavedConfigValues] = useState<Record<string, string>>()

    useEffect(() => {
        api.settings_Load().then(r => setSavedConfigValues(
            Object.fromEntries(
                r.map(c => [c.key, c.value ?? ""])
            ) as Record<string, string>
        ));
    }, [])

    const renderSettingsGroup = (): ReactNode => {
        switch (selectedSettingsGroup) {
            case "Jellyfin":
                return (<>
                    {renderSetting_Button("Sync watch history", "Sync", api.library_SyncWatchHistory)}
                    {renderSetting_ApiGroup("Jellyfin API", "Jellyfin_Url", "Jellyfin_ApiKey")}
                </>)

            case "Sonarr":
                return (<>
                    {renderSetting_Button("Sync Library", "Sync", api.library_sync)}
                    {renderSetting_ApiGroup("Sonarr API", "Sonarr_Url", "Sonarr_ApiKey")}
                </>)

            case "Providers":
                return (<>
                    {renderSetting_ApiGroup("Jikan API", "Jikan_Url", "Jikan_ApiKey")}
                    {renderSetting_ApiGroup("Id Moe API", "IdMoe_Url", "IdMoe_ApiKey")}
                </>)
        }

        return <></>
    }

    const renderSetting_Button = (label: string, btnLabel: string, action: () => Promise<void>): ReactNode => {
        return (
            <div className="Settings_Setting_Button">
                <p>{label}</p>
                <button onClick={action}>{btnLabel}</button>
            </div>
        )
    }

    const renderSetting_ApiGroup = (label: string, apiUrlKey: string, apiKeyKey: string): ReactNode => {
        if (savedConfigValues === undefined)
            return (<>LOADING...</>)

        const updateKey = (key: string, to: string) => {
            setSavedConfigValues((prev) => {
                if (prev === undefined) return prev;
                return {
                    ...prev,
                    [key]: to
                }
            })
        }

        const save = async () => {
            await Promise.all([
                api.settings_Save(apiUrlKey, savedConfigValues[apiUrlKey]),
                api.settings_Save(apiKeyKey, savedConfigValues[apiKeyKey]),
            ])
        }

        return (
            <div className="Settings_Setting_Api">
                <p>{label}</p>
                <div>
                    <p>Url</p>
                    <input onChange={e => updateKey(apiUrlKey, e.target.value)} value={savedConfigValues[apiUrlKey]}></input>
                </div>
                <div>
                    <p>Key</p>
                    <input onChange={e => updateKey(apiKeyKey, e.target.value)} value={savedConfigValues[apiKeyKey]}></input>
                </div>
                <button onClick={save}>Save</button>
            </div>
        )
    }

    return (<div className="Settings">
        {
            !modelOpen ? (
                <div className="Settings_Foldout" onClick={e => e.stopPropagation()}>
                    <div className="Settings_Foldout_Bottom">
                        <button onClick={() => setModalOpen(true)}>Settings</button>
                    </div>
                </div>

            ) :
                (
                    <div className="Settings_Menu" onClick={e => e.stopPropagation()}>
                        <header className="Settings_Menu_Header">
                            <h1>Settings</h1>
                        </header>
                        <div className="Settings_Menu_Content">
                            <aside className="Settings_Menu_Content_Sidebar">
                                {groups.map(g => <button className={g === selectedSettingsGroup ? "Selected" : ""} key={g} onClick={() => setSelectedSettingsGroup(g)}>{g}</button>)}
                            </aside>
                            <div className="Settings_Menu_Content_Entries">
                                <h2>{selectedSettingsGroup}</h2>

                                <div className="Settings_Menu_Content_Entries_Settings">
                                    {renderSettingsGroup()}
                                </div>
                            </div>
                        </div>
                    </div>
                )
        }
    </div >
    )
}