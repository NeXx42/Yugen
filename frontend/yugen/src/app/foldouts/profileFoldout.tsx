"use client"

import * as api from "@lib/api.local"
import { useRouter } from "next/navigation";

import "./profileFoldout.css"

export default function ({ isAuthenticated }: { isAuthenticated: boolean }) {
    const navigate = useRouter();

    const logout = () => {
        api.auth_Logout().finally(() => {
            navigate.push("/login")
        });
    }

    const login = () => {
        window.location.pathname = "login";
    }

    return (<div className="Profile" onClick={e => e.stopPropagation()}>
        <div className="Profile_Bottom">
            {
                isAuthenticated ? (
                    <button onClick={logout}>Logout</button>
                ) : (
                    <>
                        <button onClick={logout}>Login</button>
                    </>
                )
            }
        </div>
    </div >
    )
}