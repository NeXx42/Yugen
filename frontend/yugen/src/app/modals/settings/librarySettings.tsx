"use client"

import { useRef, useState } from "react";

import * as api from "@lib/api.local"
import { SettingsGroupProps } from "../settingsModal";

export default function (master: SettingsGroupProps) {
    const filePicker = useRef<HTMLInputElement>(null);
    const [filePickerCall, setFilePickerCallback] = useState<(() => void) | undefined>();

    const tryToImportLibrary = async () => {
        setFilePickerCallback(() => () => {
            if (filePicker?.current?.files?.[0] == undefined) {
                master.ShowToast("Invalid file", "Error")
                return;
            }

            const formData = new FormData();
            formData.append("file", filePicker!.current!.files![0]!);

            api.library_Upload(formData).then(() => {
                master.ShowToast("Imported");
            }).catch(() => {
                master.ShowToast("Failed", "Error");
            });
        })

        if (filePicker?.current == undefined) {
            master.ShowToast("File picked doesnt exist", "Error")
            return;
        }

        filePicker.current.accept = ".txt";
        filePicker.current.click();
    }


    return (<>
        <input ref={filePicker} type="file" hidden onChange={() => filePickerCall?.()} />

        <h2>Import / Export</h2>
        <div className="Settings_Menu_Content_Entries_Settings">
            {master.Button("Import Library", "Import", "", tryToImportLibrary)}
        </div >
    </>)
}