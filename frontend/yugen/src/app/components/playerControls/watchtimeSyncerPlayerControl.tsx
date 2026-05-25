"use client"

import * as api from "@lib/api.local"
import { selectPlayback, selectTime, usePlayer } from "@videojs/react";
import { useEffect, useState } from "react";

export default function ({ syncFunc }: { syncFunc: (runtime: number, percentage: number) => void }) {
    const { paused, currentTime, duration } = usePlayer((s) => ({
        paused: s.paused as boolean,
        currentTime: s.currentTime as number | null,
        duration: s.duration as number | null,
    }));

    const interval = 10;
    const [lastUpdate, setLastUpdated] = useState<number | null>(null)

    const syncTime = () => {
        if (currentTime == null || duration == null || duration == 0)
            return;

        syncFunc(currentTime, currentTime / duration);
        setLastUpdated(currentTime);
    }

    useEffect(() => {
        if (currentTime == null)
            return;

        if (lastUpdate === null || Math.abs(currentTime - lastUpdate) >= interval)
            syncTime();
    }, [currentTime]);

    useEffect(() => {
        if (paused)
            syncTime();

    }, [paused])

    return <></>;
}