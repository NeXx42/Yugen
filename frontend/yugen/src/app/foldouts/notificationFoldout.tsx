"use client"

import * as api from "@lib/api.local"
import { UserNotification } from "@shared/types"

import { useEffect, useState } from "react"
import "./notificationFoldout.css"

export default function () {
    const [notifications, setNotifications] = useState<UserNotification[] | undefined>()

    useEffect(() => {
        api.notification_Get().then(r => setNotifications(r.sort((a, b) => a.order - b.order)));
    }, [])

    return (<div className="Notifications">
        <div className="Notifications_Header">
            <h1>Notifications</h1>
        </div>
        <div className="Notifications_Filter">
        </div>
        <div className="Notifications_Notifs">
            {
                notifications == undefined ? (
                    <>Loading</>
                ) : (
                    notifications.map(n => {
                        return (<div key={n.id}>
                            <img src={n.icon} />
                            <div>
                                <a>{n.title}</a>
                                <p>{n.eventName}</p>
                            </div>
                        </div>)
                    })
                )
            }
        </div>
    </div>)
}