"use client"

import * as api from "@lib/api.local"
import { SettingsGroupProps } from "../settingsModal";

export default function (master: SettingsGroupProps) {
    const librarySyncCallback = async () => {
        try {
            const importCount = await api.library_sync();
            master.ShowToast(`Imported ${importCount}`);
        } catch {
            master.ShowToast("Failed import", "Error");
        }
    }

    return (<>
        <h2>Metadata</h2>
        <div className="Settings_Menu_Content_Entries_Settings">
            {master.Dropdown("Metadata provider", "MetadataProviderId", ["Anilist", "Tenrai"])}
            {master.Button("Update links", "Download", "", api.catalog_ReloadLinks)}
        </div >

        <h2>Jellyfin</h2>
        <div className="Settings_Menu_Content_Entries_Settings">
            {master.ApiGroup("Jellyfin API", "Jellyfin_Url", "Jellyfin_ApiKey")}
        </div >

        <h2>Indexers</h2>
        <div className="Settings_Menu_Content_Entries_Settings">
            {master.Button("Sync Downloads", "Sync", "", librarySyncCallback)}
            {master.ApiGroup("Sonarr API", "Sonarr_Url", "Sonarr_ApiKey")}
            {master.ApiGroup("Radarr API", "Radarr_Url", "Radarr_ApiKey")}
        </div >

    </>)

}