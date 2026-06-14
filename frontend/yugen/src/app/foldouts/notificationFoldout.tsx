import * as api from "@lib/api.local"

import { UserNotification } from "@shared/types"
import { ReactNode, useEffect, useState } from "react"

import "./notificationFoldout.css"
import NotificationElement from "./notificationElement";

const CategorySourceLookup: Record<NotificationSourceFilter, string[]> = {
    Airing: ["System"],
    System: ["System"],
    Library: ["Sonarr", "Radarr"],
    All: []
}
type NotificationSourceFilter = "Airing" | "Library" | "System" | "All";

export default function ({ refreshNumber }: { refreshNumber: (count: number) => void }) {
    const [loading, setLoading] = useState(true);
    const [notifications, setNotifications] = useState<UserNotification[] | undefined>()
    const [currentFilter, setCurrentFilter] = useState<NotificationSourceFilter>("Airing");

    const drawNotification = (): ReactNode => {
        return notifications?.filter(n => {
            switch (currentFilter.toLowerCase()) {
                case "airing":
                    return n.eventName === "New Episode";

                case "system":
                    return n.source === "System";

                case "library":
                    return n.source === "Sonarr" || n.source === "Radarr";

                default:
                    return true;
            }

        }).map(n =>
            <NotificationElement key={n.id} notification={n} />
        )
    }

    const clearRead = () => api.notification_ClearRead().then(refreshNotifications);
    const readAll = () => api.notification_MarkAllAsRead(CategorySourceLookup[currentFilter]).then(refreshNotifications);

    const refreshNotifications = () => {
        setLoading(true);
        api.notification_Get()
            .then(r => {
                refreshNumber(r.length);
                setNotifications(r.sort((a, b) => b.time - a.time))
            })
            .finally(() => setLoading(false));
    }

    useEffect(refreshNotifications, [])

    return (<div className="Notifications" onClick={e => e.stopPropagation()}>
        <div className="Notifications_Header">
            <h2>Notifications</h2>
            <div>
                <button onClick={readAll}>Mark {currentFilter} as Read</button>
                <button onClick={clearRead}>Clear read</button>
            </div>
        </div>
        <div className="Notifications_Filter">
            <button onClick={() => setCurrentFilter("Airing")} className={currentFilter === "Airing" ? "Selected" : ""}>
                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 640 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M150.94 192h33.73c11.01 0 18.61-10.83 14.86-21.18-4.93-13.58-7.55-27.98-7.55-42.82s2.62-29.24 7.55-42.82C203.29 74.83 195.68 64 184.67 64h-33.73c-7.01 0-13.46 4.49-15.41 11.23C130.64 92.21 128 109.88 128 128c0 18.12 2.64 35.79 7.54 52.76 1.94 6.74 8.39 11.24 15.4 11.24zM89.92 23.34C95.56 12.72 87.97 0 75.96 0H40.63c-6.27 0-12.14 3.59-14.74 9.31C9.4 45.54 0 85.65 0 128c0 24.75 3.12 68.33 26.69 118.86 2.62 5.63 8.42 9.14 14.61 9.14h34.84c12.02 0 19.61-12.74 13.95-23.37-49.78-93.32-16.71-178.15-.17-209.29zM614.06 9.29C611.46 3.58 605.6 0 599.33 0h-35.42c-11.98 0-19.66 12.66-14.02 23.25 18.27 34.29 48.42 119.42.28 209.23-5.72 10.68 1.8 23.52 13.91 23.52h35.23c6.27 0 12.13-3.58 14.73-9.29C630.57 210.48 640 170.36 640 128s-9.42-82.48-25.94-118.71zM489.06 64h-33.73c-11.01 0-18.61 10.83-14.86 21.18 4.93 13.58 7.55 27.98 7.55 42.82s-2.62 29.24-7.55 42.82c-3.76 10.35 3.85 21.18 14.86 21.18h33.73c7.02 0 13.46-4.49 15.41-11.24 4.9-16.97 7.53-34.64 7.53-52.76 0-18.12-2.64-35.79-7.54-52.76-1.94-6.75-8.39-11.24-15.4-11.24zm-116.3 100.12c7.05-10.29 11.2-22.71 11.2-36.12 0-35.35-28.63-64-63.96-64-35.32 0-63.96 28.65-63.96 64 0 13.41 4.15 25.83 11.2 36.12l-130.5 313.41c-3.4 8.15.46 17.52 8.61 20.92l29.51 12.31c8.15 3.4 17.52-.46 20.91-8.61L244.96 384h150.07l49.2 118.15c3.4 8.16 12.76 12.01 20.91 8.61l29.51-12.31c8.15-3.4 12-12.77 8.61-20.92l-130.5-313.41zM271.62 320L320 203.81 368.38 320h-96.76z" />
                </svg>
                Airing
            </button>
            <button onClick={() => setCurrentFilter("Library")} className={currentFilter === "Library" ? "Selected" : ""}>
                <svg viewBox="0 0 24 24" fill="currentColor" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M0 0h24v24H0V0z" fill="none" />
                    <path d="M0 0h24v24H0V0z" fill="none" />
                    <path d="M4 6H2v14c0 1.1.9 2 2 2h14v-2H4V6z" />
                    <path d="M20 2H8c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm0 10l-2.5-1.5L15 12V4h5v8z" />
                </svg>
                Library
            </button>
            <button onClick={() => setCurrentFilter("System")} className={currentFilter === "System" ? "Selected" : ""}>
                <svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z" />
                </svg>
                System
            </button>
            <button onClick={() => setCurrentFilter("All")} className={currentFilter === "All" ? "Selected" : ""}>
                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 448 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                    <path d="M224 512c35.32 0 63.97-28.65 63.97-64H160.03c0 35.35 28.65 64 63.97 64zm215.39-149.71c-19.32-20.76-55.47-51.99-55.47-154.29 0-77.7-54.48-139.9-127.94-155.16V32c0-17.67-14.32-32-31.98-32s-31.98 14.33-31.98 32v20.84C118.56 68.1 64.08 130.3 64.08 208c0 102.3-36.15 133.53-55.47 154.29-6 6.45-8.66 14.16-8.61 21.71.11 16.4 12.98 32 32.1 32h383.8c19.12 0 32-15.6 32.1-32 .05-7.55-2.61-15.27-8.61-21.71z" />
                </svg>
                All
            </button>
        </div>
        <div className="Notifications_Notifs">
            {
                loading ? (
                    <>
                        <div className="Notification_Notifs_Skeleton" />
                        <div className="Notification_Notifs_Skeleton" />
                        <div className="Notification_Notifs_Skeleton" />
                        <div className="Notification_Notifs_Skeleton" />
                        <div className="Notification_Notifs_Skeleton" />
                        <div className="Notification_Notifs_Skeleton" />
                        <div className="Notification_Notifs_Skeleton" />
                    </>
                ) : (drawNotification())
            }
        </div>
    </div>)
}