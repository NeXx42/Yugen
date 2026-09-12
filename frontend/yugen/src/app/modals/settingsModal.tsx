"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useRef, useState } from "react";
import { useToast } from "@/app/context/toast"

import "./settingsModal.css"
import { useModals } from "../context/modalContext";
import LoadingModal from "./loadingModal";


type SettingsGroup = "App" | "External" | "Library";
const groups: SettingsGroup[] = ["App", "External", "Library"];


export default function () {
    const { showToast } = useToast();
    const { showModal, closeModal } = useModals();

    const filePicker = useRef<HTMLInputElement>(null);


    const [selectedSettingsGroup, setSelectedSettingsGroup] = useState<SettingsGroup>(groups[0]);
    const [savedConfigValues, setSavedConfigValues] = useState<Record<string, string>>()

    const [filePickerCall, setFilePickerCallback] = useState<(() => void) | undefined>();

    const loadApiRequest = (loading: () => Promise<any>) => {
        showModal(<LoadingModal closeRequest={closeModal} loadingCall={loading} />)
    }

    useEffect(() => {
        api.settings_Load().then(r => setSavedConfigValues(
            Object.fromEntries(
                r.map(c => [c.key, c.value ?? ""])
            ) as Record<string, string>
        ));
    }, [])

    const tryToImportLibrary = async () => {
        setFilePickerCallback(() => () => {
            if (filePicker?.current?.files?.[0] == undefined) {
                showToast("Invalid file", "Error")
                return;
            }

            const formData = new FormData();
            formData.append("file", filePicker!.current!.files![0]!);

            api.library_Upload(formData).then(() => {
                showToast("Imported");
            }).catch(() => {
                showToast("Failed", "Error");
            });
        })

        if (filePicker?.current == undefined) {
            showToast("File picked doesnt exist", "Error")
            return;
        }

        filePicker.current.accept = ".txt";
        filePicker.current.click();
    }

    const renderSettingsGroup = (): ReactNode => {
        switch (selectedSettingsGroup) {
            case "App":
                return (
                    <>
                        <h2>Settings</h2>
                        <div className="Settings_Menu_Content_Entries_Settings">
                            {renderSetting_Toggle("Allow adult content", "AdultContent")}
                        </div>

                        <h2>Caching</h2>
                        <div className="Settings_Menu_Content_Entries_Settings">
                            {renderSetting_Button("Clear cache", "Clear", "Negative", api.catalog_ClearCache)}
                            {renderSetting_Button("Clear database cache", "Clear", "Negative", api.catalog_ClearDatabase)}
                        </div>
                    </>)

            case "External":
                const librarySyncCallback = async () => {
                    try {
                        const importCount = await api.library_sync();
                        showToast(`Imported ${importCount}`);
                    } catch {
                        showToast("Failed import", "Error");
                    }
                }
                return (<>
                    <h2>Metadata</h2>
                    <div className="Settings_Menu_Content_Entries_Settings">
                        {renderSettings_Dropdown("Metadata provider", "MetadataProviderId", ["Anilist", "Tenrai"])}
                        {renderSetting_Button("Update links", "Download", "", api.catalog_ReloadLinks)}
                    </div >

                    <h2>Jellyfin</h2>
                    <div className="Settings_Menu_Content_Entries_Settings">
                        {renderSetting_ApiGroup("Jellyfin API", "Jellyfin_Url", "Jellyfin_ApiKey")}
                    </div >

                    <h2>Indexers</h2>
                    <div className="Settings_Menu_Content_Entries_Settings">
                        {renderSetting_Button("Sync Downloads", "Sync", "", librarySyncCallback)}
                        {renderSetting_ApiGroup("Sonarr API", "Sonarr_Url", "Sonarr_ApiKey")}
                        {renderSetting_ApiGroup("Radarr API", "Radarr_Url", "Radarr_ApiKey")}
                    </div >

                </>)

            case "Library":
                return (<>
                    <h2>Import / Export</h2>
                    <div className="Settings_Menu_Content_Entries_Settings">
                        {renderSetting_Button("Import Library", "Import", "", tryToImportLibrary)}
                    </div >
                </>)
        }

        return <></>
    }

    const renderSetting_Toggle = (label: string, configKey: string): ReactNode => {
        if (savedConfigValues === undefined)
            return (<>LOADING...</>)

        const configValue: boolean = savedConfigValues![configKey] === "1";

        const btnIntercept = async () => {
            try {
                const saveValue = (!configValue) ? "1" : "0";

                setSavedConfigValues((prev) => {
                    if (prev === undefined) return prev;
                    return {
                        ...prev,
                        [configKey]: saveValue
                    }
                });

                await api.settings_Save(configKey, saveValue);
                showToast("Updated");
            }
            catch {
                showToast("Failed", "error");
            }
        }

        return (
            <div className="Settings_Setting_Button">
                <p>{label}</p>
                <button onClick={btnIntercept}>{configValue ? "Enabled" : "Disabled"}</button>
            </div>
        )
    }

    const renderSetting_Button = (label: string, btnLabel: string, btnClass: string, action: () => Promise<void>, suppressToast: boolean = false): ReactNode => {
        const btnIntercept = async () => {
            try {
                loadApiRequest(() => action());

                if (suppressToast)
                    showToast("Success", "success");
            }
            catch {
                showToast("Failed", "error");
            }
        }

        return (
            <div className="Settings_Setting_Button">
                <p>{label}</p>
                <button className={btnClass} onClick={btnIntercept}>{btnLabel}</button>
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
            try {
                await Promise.all([
                    api.settings_Save(apiUrlKey, savedConfigValues[apiUrlKey]),
                    api.settings_Save(apiKeyKey, savedConfigValues[apiKeyKey]),
                ])

                showToast("Saved", "Success");
            }
            catch {
                showToast("Error", "Error");
            }
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

    const renderSettings_Dropdown = (label: string, optionKey: string, options: string[]): ReactNode => {
        if (savedConfigValues === undefined)
            return (<>LOADING...</>)

        const updateKey = async (to: string) => {
            await setSavedConfigValues((prev) => {
                if (prev === undefined) return prev;
                return {
                    ...prev,
                    [optionKey]: to
                }
            })

            try {
                await api.settings_Save(optionKey, to);
                showToast("Saved", "success");
            }
            catch {
                showToast("Error", "Error");
            }
        }

        return (
            <div className="Settings_Setting_Dropdown">
                <p>{label}</p>
                <select value={savedConfigValues[optionKey]} onChange={e => updateKey(e.target.value)}>
                    {options.map((o, i) => <option key={i} value={i}>{o}</option>)}
                </select>
            </div>
        )
    }


    return (
        <div className="Settings_Menu" onClick={e => e.stopPropagation()}>
            <input ref={filePicker} type="file" hidden onChange={() => filePickerCall?.()} />

            <header className="Settings_Menu_Header">
                <h1>Settings</h1>
            </header>
            <div className="Settings_Menu_Content">
                <aside className="Settings_Menu_Content_Sidebar">
                    {groups.map(g => <button className={g === selectedSettingsGroup ? "Selected" : ""} key={g} onClick={() => setSelectedSettingsGroup(g)}>{g}</button>)}
                </aside>
                <div className="Settings_Menu_Content_Entries">
                    <div>
                        {renderSettingsGroup()}
                    </div>
                </div>
            </div>
        </div>
    )
}