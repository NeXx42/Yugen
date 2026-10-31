"use client"

import * as api from "@lib/api.local"
import { SettingsGroupProps } from "../settingsModal"

export default function (master: SettingsGroupProps) {

    return (
        <>
            <h2>Settings</h2>
            <div className="Settings_Menu_Content_Entries_Settings">
                {master.Toggle("Allow adult content", "AdultContent")}
            </div>

            <h2>Caching</h2>
            <div className="Settings_Menu_Content_Entries_Settings">
                {master.Button("Clear cache", "Clear", "Negative", api.catalog_ClearCache)}
                {master.Button("Clear database cache", "Clear", "Negative", api.catalog_ClearDatabase)}
            </div>
        </>)
}