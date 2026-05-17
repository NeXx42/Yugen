"use client"

import * as api from "@lib/api.local"
import { useRouter } from "next/navigation";

import "./profileFoldout.css"

export default function () {
    const navigate = useRouter();

    const logout = () => {
        api.auth_Logout().finally(() => {
            navigate.push("/login")
        });
    }

    return (<div className="Profile" onClick={e => e.stopPropagation()}>
        <div className="Profile_Bottom">
            <button onClick={logout}>Logout</button>
        </div>
    </div >
    )
}