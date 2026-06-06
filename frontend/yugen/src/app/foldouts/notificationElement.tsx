"use client"

import { UserNotification } from "../shared/types";
import "./notificationElement.css"

export default function ({ notification }: { notification: UserNotification }) {
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

    const markAsRead = () => {
        navigator.sendBeacon(`api/Notifications/${notification.id}/Read`);
    }

    return (
        <a href={notification.url} onClick={markAsRead} className={`Notification ${notification.hasBeenSeen ? "Seen" : ""}`} style={{ "--hover-color": notification?.media?.colour ?? "white" } as React.CSSProperties}>
            <div >
                {notification?.media?.banner &&
                    <div className="Notification_BG">
                        <img src={notification?.media?.banner} />
                        <div />
                    </div>
                }

                <img src={notification?.media?.cardImg} />

                <div className="Notification_Info">
                    <h5>{notification?.media?.title ?? notification.eventName}</h5>
                    <div>
                        <p>{notification.eventName}</p>
                        <p>{formatUnixToDayMonth(notification.time)}</p>
                    </div>
                </div>

            </div >
        </a>
    )
}