"use client"

import * as api from "@lib/api.local"

import { ReactNode, useEffect, useState } from "react"
import "./page.css"
import { MediaCardInfo } from "@/app/shared/types";
import MediaCardHorizontal from "@/app/components/mediaCardHorizontal";
import MediaCardHorizontalSkeleton from "@/app/components/mediaCardHorizontalSkeleton";

const daysOfTheWeek = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
];

const monthsOfTheYear = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December"
]

const getDayName = (date: Date): string => {
    return daysOfTheWeek[date.getDay()];
}

export default function () {
    const today = new Date(Date.now());
    const [selectedDay, setSelectedDay] = useState(today.getDate());

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | undefined>(undefined);
    const [pageContent, setPageContent] = useState<Partial<Record<number, MediaCardInfo[]>>>([])

    useEffect(() => {

        setLoading(true);
        api.catalog_UpcomingForDay(selectedDay)
            .then(res => {
                setPageContent(Object.groupBy(res, res => new Date(res.nextReleaseDate! * 1000).getHours()))
            })
            .catch(e => setError(e.message))
            .finally(() => setLoading(false));
    }, [selectedDay])


    const drawDateSelection = (dayOfWeek: number): ReactNode => {
        const daysUntil = (dayOfWeek - today.getUTCDay() + 7) % 7;
        const representingDate = new Date(today);

        representingDate.setUTCDate(
            today.getUTCDate() + daysUntil
        );

        return (
            <div className="Schedule_Dates_Day" onClick={() => setSelectedDay(representingDate.getDate())}>
                <a className={selectedDay === representingDate.getDate() ? "Selected" : ""}>{getDayName(representingDate)}</a>
                {selectedDay === representingDate.getDate() && <p>{monthsOfTheYear[representingDate.getMonth()]} {representingDate.getUTCDate()}</p>}
            </div>
        );
    }

    const drawGroup = (key: string, data: MediaCardInfo[]): ReactNode => {
        return (
            <div key={key} className="Schedule_Content_Container">
                <div className="Schedule_Content_Header">
                    <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 320 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                        <path d="M278.6 233.4c12.5 12.5 12.5 32.8 0 45.3l-160 160c-12.5 12.5-32.8 12.5-45.3 0s-12.5-32.8 0-45.3L210.7 256 73.4 118.6c-12.5-12.5-12.5-32.8 0-45.3s32.8-12.5 45.3 0l160 160z" />
                    </svg>
                    <a>{key}:00</a>
                </div>
                <div className="Schedule_Content_Entries">
                    {data.map(d => <MediaCardHorizontal key={d.aniListId} card={d} />)}
                </div>
            </div>
        )
    }

    return (
        <div className="Schedule">
            <div className="Schedule_Dates">
                {drawDateSelection(0)}
                <p>/</p>
                {drawDateSelection(1)}
                <p>/</p>
                {drawDateSelection(2)}
                <p>/</p>
                {drawDateSelection(3)}
                <p>/</p>
                {drawDateSelection(4)}
                <p>/</p>
                {drawDateSelection(5)}
                <p>/</p>
                {drawDateSelection(6)}
            </div>

            <div className="Schedule_Content">
                {
                    loading ?
                        (
                            <>
                                <MediaCardHorizontalSkeleton />
                                <MediaCardHorizontalSkeleton />
                                <MediaCardHorizontalSkeleton />
                            </>
                        )
                        : (
                            Object.entries(pageContent).map(([key, value]) =>
                                drawGroup(key, value ?? [])
                            )
                        )
                }
            </div>
        </div>
    )
}