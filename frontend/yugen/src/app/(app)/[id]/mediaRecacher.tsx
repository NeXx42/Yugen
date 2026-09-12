"use client"

import * as api from "@lib/api.local"

import { MediaInfo } from "@/app/shared/types";
import { useEffect, useRef, useState } from "react";

import "./mediaRecacher.css"

export default function ({ media }: { media: MediaInfo }) {
    const ref = useRef<HTMLDivElement>(null);
    const [refreshMenuOpen, setRefreshMenuOpen] = useState(false);

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (ref.current && !ref.current.contains(event.target as Node)) {
                setRefreshMenuOpen(false);
            }
        }

        document.addEventListener("mousedown", handleClickOutside);

        return () => {
            document.removeEventListener(
                "mousedown",
                handleClickOutside
            );
        };
    });

    return (<div className="MediaRecacher">
        <button onClick={() => setRefreshMenuOpen(true)}>
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                <path d="M3 9.5a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3m5 0a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3m5 0a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3" />
            </svg>
        </button>

        {
            refreshMenuOpen && (<div className="MediaRecacher_Container" ref={ref}>
                <button onClick={() => api.catalog_RecacheRecommended(media.id).then(() => window.location.reload())}>Refetch Recommendations</button>
            </div>)
        }
    </div>)
}