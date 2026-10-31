"use client"

import * as api from "@lib/api.local"

import { ComponentType, ReactNode, useEffect, useState } from "react";
import { useToast } from "@/app/context/toast"

import "./settingsModal.css"
import { useModals } from "../context/modalContext";
import LoadingModal from "./loadingModal";

import appSettings from "./settings/appSettings";
import externalSettings from "./settings/externalSettings";
import librarySettings from "./settings/librarySettings";
import userSettings from "./settings/userSettings";


export interface SettingsGroupProps {
    Button: (label: string, btnLabel: string, btnClass: string, action: () => Promise<void>, suppressToast?: boolean) => ReactNode;
    Toggle: (label: string, configKey: string) => ReactNode;
    ApiGroup: (label: string, apiUrlKey: string, apiKeyKey: string) => ReactNode;
    Dropdown: (label: string, optionKey: string, options: string[], callback?: (to: string) => Promise<void>) => ReactNode;

    ShowToast: (msg: string, type?: string) => void;
}

interface SettingsGroup {
    type: SettingsGroupType;
    node: ComponentType<SettingsGroupProps>;
}

type SettingsGroupType = "App" | "External" | "Library" | "User";
const groups: SettingsGroup[] = [
    {
        type: "App",
        node: appSettings
    },
    {
        type: "External",
        node: externalSettings
    },
    {
        type: "Library",
        node: librarySettings
    },
    {
        type: "User",
        node: userSettings
    },
];


export default function () {
    const { showToast } = useToast();
    const { showModal, closeModal } = useModals();

    const [selectedSettingsGroup, setSelectedSettingsGroup] = useState<SettingsGroupType>("App");
    const [savedConfigValues, setSavedConfigValues] = useState<Record<string, string>>();

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

    const renderSettingsGroup = (): ReactNode => {
        const SettingGroupType = groups.filter(g => g.type === selectedSettingsGroup)[0].node
        return <SettingGroupType Button={renderSetting_Button} Toggle={renderSetting_Toggle} ApiGroup={renderSetting_ApiGroup} Dropdown={renderSettings_Dropdown} ShowToast={showToast} />
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

    const renderSettings_Dropdown = (label: string, optionKey: string, options: string[], callback?: (value: string) => void): ReactNode => {
        if (savedConfigValues === undefined)
            return (<>LOADING...</>)

        const updateKey = async (to: string) => {
            if (callback) {
                callback(to);
                return;
            }

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

            <header className="Settings_Menu_Header">
                <h1>Settings</h1>
            </header>
            <div className="Settings_Menu_Content">
                <aside className="Settings_Menu_Content_Sidebar">
                    {groups.map(g => <button className={g.type === selectedSettingsGroup ? "Selected" : ""} key={g.type} onClick={() => setSelectedSettingsGroup(g.type)}>{g.type}</button>)}
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