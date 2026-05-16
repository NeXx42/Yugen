import { ReactNode, useState } from "react"
import "./settingsFoldout.css"

type SettingsGroup = "Jellyfin" | "Sonarr" | "Providers";

export default function () {
    const [modelOpen, setModalOpen] = useState(false);

    const [selectedSettingsGroup, setSelectedSettingsGroup] = useState<SettingsGroup>("Jellyfin");
    const groups: SettingsGroup[] = ["Jellyfin", "Sonarr", "Providers"];

    const renderSettingsGroup = (): ReactNode => {
        switch (selectedSettingsGroup) {
            case "Jellyfin":
                return (<>
                    {renderSetting_Button("Sync watch history", "Sync")}
                </>)

            case "Sonarr":
                return (<>
                    {renderSetting_Button("Sync Library", "Sync")}
                </>)
        }

        return <></>
    }

    const renderSetting_Button = (label: string, btnLabel: string): ReactNode => {
        return (
            <div className="Settings_Setting_Button">
                <p>{label}</p>
                <button>{btnLabel}</button>
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