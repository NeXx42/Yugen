"use client"

import * as api from "@lib/api.local"

import { useEffect, useState } from "react"
import { SettingsGroupProps } from "../settingsModal"
import { useUser } from "@/app/context/userContext";
import { LanguageCodes, Languages } from "@/app/shared/enum_Languages";
import { UserSettingTypes } from "@/app/shared/enum_UserSettings";


type UserSettingsValueTypes = "LanguageSelector" | "LanguageSelectorWithNone";

interface UserSetting {
    type: UserSettingTypes;
    valueType: UserSettingsValueTypes;
}

const groupedSettings: Record<string, UserSetting[]> = {
    "Language": [
        {
            type: "PreferredAudioLanguage",
            valueType: "LanguageSelector"
        },
        {
            type: "PreferredSubtitleLanguage",
            valueType: "LanguageSelectorWithNone"
        }
    ]
}

export default function (master: SettingsGroupProps) {
    const { session, updateSessionSettings } = useUser();

    if (!session)
        return <></>

    const [userSettings, setUserSettings] = useState<Record<string, string>>();

    useEffect(() => {
        setUserSettings(session.settings);
    }, [])

    const formatTypeName = (type: UserSettingTypes) => {
        return type.replace(/([a-z])([A-Z])/g, "$1 $2");
    }

    const updateUserSetting = (key: string, value: string, instantSave?: boolean) => {
        setUserSettings(prev => {
            return {
                ...(prev ?? {}),
                [key]: value
            }
        });

        if (instantSave ?? false) {
            userSettings![key] = value; // have to do this because the setUsers isnt instant
            saveSetting(key);
        }
    }

    const saveSetting = (key: string) => {
        const value = userSettings![key];

        api.auth_SaveUserSetting(key, value)
            .catch(e => master.ShowToast(e, "Error"))
            .then(() => {
                updateSessionSettings(userSettings!);
                master.ShowToast("Updated");
            })
    }

    return Object.keys(groupedSettings).map(s =>
        <>
            <h2>{s}</h2>
            <div className="Settings_Menu_Content_Entries_Settings">
                {
                    groupedSettings[s].map(setting => {
                        const currentValue: string | undefined = userSettings?.[setting.type];

                        switch (setting.valueType) {

                            case "LanguageSelector":
                            case "LanguageSelectorWithNone":
                                return (
                                    <div key={setting.type} className="Settings_Setting_Dropdown">
                                        <p>{formatTypeName(setting.type)}</p>
                                        <select value={currentValue ?? LanguageCodes[0]} onChange={e => updateUserSetting(setting.type, e.target.value, true)}>
                                            {setting.valueType === "LanguageSelectorWithNone" && <option value={"None"}>None</option>}
                                            {LanguageCodes.map((o, i) => <option key={i} value={o}>{Languages[o]}</option>)}
                                        </select>
                                    </div>
                                )

                            default:
                                return <>Unknown Setting type</>;
                        }

                        return <></>
                    })
                }
            </div>
        </>
    )
}