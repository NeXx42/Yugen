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

        return (<div className={n.hasBeenSeen ? "Seen" : ""} key={n.id} onClick={redirect}>
            <img src={n.icon} />
            <div>
                <a>{n.title}</a>
                <p>{n.eventName}</p>
                <a>{n.time}</a>
            </div>
        </div>)
    }

    const clearRead = () => {
        api.notification_ClearRead().then(refreshNotifications);
    }

    const refreshNotifications = () => api.notification_Get().then(r => setNotifications(r.sort((a, b) => b.time - a.time)));

    return (<div className="Notifications">
        <div className="Notifications_Header">
            <h1>Notifications</h1>
            <button onClick={clearRead}>Clear read</button>
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