"use client"

import * as api from "@lib/api.local"

import { useRouter } from "next/navigation";
import { UserNotification } from "@shared/types"
import { useEffect, useState } from "react"

import "./notificationFoldout.css"

export default function () {
    const navigate = useRouter();
    const [notifications, setNotifications] = useState<UserNotification[] | undefined>()

    useEffect(() => {
        refreshNotifications();
    }, [])

    const drawNotification = (n: UserNotification) => {
        const redirect = async () => {
            void api.notification_Read(n.id);

            if (n.url == null) return;
            navigate.push(n.url);
        }

        return (<div className={`Notifications_Notifs_Notification ${n.hasBeenSeen ? "Seen" : ""}`} key={n.id} onClick={redirect}>
            {n.bannerIcon &&
                <div className="Notifications_Notifs_Notification_BG">
                    <img src={n.bannerIcon} />
                    <div />
                </div>
            }

            <img src={n.icon} />

            <div className="Notifications_Notifs_Notification_Info">
                <a>{n.title}</a>
                <div>
                    <p>{n.eventName}</p>
                    <a>{formatUnixToDayMonth(n.time)}</a>
                </div>
            </div>

        </div>)
    }

    function formatUnixToDayMonth(unixMilliSeconds: number) {
        const date = new Date(unixMilliSeconds);

        const day = date.getDate();
        const monthNames = [
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        ];

        const month = monthNames[date.getMonth()];
        return `${day} ${month}`;
    }

    const clearRead = () => {
        api.notification_ClearRead().then(refreshNotifications);
    }

    const readAll = () => {
        api.notification_MarkAllAsRead().then(refreshNotifications);
    }

    const refreshNotifications = () => api.notification_Get().then(r => setNotifications(r.sort((a, b) => b.time - a.time)));

    return (<div className="Notifications" onClick={e => e.stopPropagation()}>
        <div className="Notifications_Header">
            <h1>Notifications</h1>
            <div>
                <button onClick={readAll}>Mark All as Read</button>
                <button onClick={clearRead}>Clear read</button>
            </div>
        </div>
        <div className="Notifications_Filter">
        </div>
        <div className="Notifications_Notifs">
            {
                notifications == undefined ? (
                    <>Loading</>
                ) : (
                    notifications.map(drawNotification)
                )
            }
        </div>
    </div>)
}