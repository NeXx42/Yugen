"use client"

import { createPortal } from "react-dom";
import "./toast.css";

import { createContext, useContext, useState } from "react"

type Toast = {
    id: number
    message: string
    type: "success" | "error"
}

const ToastContext = createContext<any>(null)

export function ToastProvider({ children }: { children: React.ReactNode }) {
    const [toasts, setToasts] = useState<Toast[]>([])

    const showToast = (message: string, type: Toast["type"] = "success") => {
        const id = Date.now()
        setToasts((prev) => [...prev, { id, message, type }])

        setTimeout(() => {
            setToasts((prev) => prev.filter((t) => t.id !== id))
        }, 1500)
    }

    return (
        <ToastContext.Provider value={{ showToast }}>
            {children}

            {createPortal((
                <div className="ToastContainer">
                    {toasts.sort((a, b) => a.id - b.id).slice(0, 10).map((t) => (
                        <div key={t.id} className={`Toast ${t.type === "error" ? "Error" : ""}`}>{t.message}</div>
                    ))}
                </div>
            ), document.body)}
        </ToastContext.Provider>
    )
}

export const useToast = () => useContext(ToastContext)