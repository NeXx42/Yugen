"use client"

import * as api from "@lib/api.local"
import { useRouter } from "next/navigation";

import "./profileFoldout.css"
import { useModals } from "../context/modalContext";
import { useEffect } from "react";
import LoginModal from "../modals/loginModal";

export default function ({ isAuthenticated }: { isAuthenticated: boolean }) {
    if (!isAuthenticated) {
        const { showModal, closeModal } = useModals();

        useEffect(() => {
            closeModal();
            showModal(<LoginModal />)
        }, [])

        return;
    }

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
        <div style={{ gridColumn: "span 2" }}>
            <button onClick={logout}>Logout</button>
        </div>
    </div >
    )
}