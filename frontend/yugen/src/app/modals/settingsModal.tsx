"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useRef, useState } from "react";
import { useToast } from "@/app/context/toast"

import "./settingsModal.css"
import { useModals } from "../context/modalContext";
import LoadingModal from "./loadingModal";


type SettingsGroup = "App" | "Jellyfin" | "Sonarr" | "Providers";


export default function () {
    const { showToast } = useToast();
    const { showModal, closeModal } = useModals();

    const filePicker = useRef<HTMLInputElement>(null);

    const groups: SettingsGroup[] = ["App", "Jellyfin", "Sonarr", "Providers"];

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
                return (<>
                    {renderSetting_Button("Import Library", "Import", "", tryToImportLibrary)}
                    {renderSetting_Toggle("Allow adult content", "AdultContent")}

                    {renderSetting_Button("Clear cache", "Clear", "Negative", api.catalog_ClearCache)}
                    {renderSetting_Button("Clear database cache", "Clear", "Negative", api.catalog_ClearDatabase)}
                </>)

            case "Jellyfin":
                return (<>
                    {renderSetting_Button("Sync watch history", "Sync", "", api.library_SyncWatchHistory)}
                    {renderSetting_ApiGroup("Jellyfin API", "Jellyfin_Url", "Jellyfin_ApiKey")}
                </>)

            case "Sonarr":
                const librarySyncCallback = async () => {
                    try {
                        const importCount = await api.library_sync();
                        showToast(`Imported ${importCount}`);
                    } catch {
                        showToast("Failed import", "Error");
                    }
                }

                return (<>
                    {renderSetting_Button("Sync Library", "Sync", "", librarySyncCallback)}
                    {renderSetting_ApiGroup("Sonarr API", "Sonarr_Url", "Sonarr_ApiKey")}
                </>)

            case "Providers":
                return (<>
                    {renderSetting_Button("Download links", "Download", "", api.catalog_ReloadLinks)}
                    {renderSetting_ApiGroup("Jikan API", "Jikan_Url", "Jikan_ApiKey")}
                    {renderSetting_ApiGroup("Id Moe API", "IdMoe_Url", "IdMoe_ApiKey")}
                </>)
        }

        return <></>
    }

    const renderSetting_Toggle = (label: string, configKey: string): ReactNode => {
        if (savedConfigValues === undefined)
            return (<>LOADING...</>)

        const configValue: boolean = savedConfigValues![configKey] === "1";

        const btnIntercept = async () => {
            console.log("test");
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

                showToast("Saved", "success");
            }
            catch {
                showToast("Error", "error");
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
                    <h2>{selectedSettingsGroup}</h2>

                    <div className="Settings_Menu_Content_Entries_Settings">
                        {renderSettingsGroup()}
                    </div>
                </div>
            </div>
        </div>
    )
}