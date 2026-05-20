"use client"

import "./trendingList.css"

import { MediaInfo } from "@/app/shared/types"
import { useEffect, useRef, useState } from "react"

import { useRouter } from "next/navigation"

export default function ({ data }: { data: MediaInfo[] }) {

    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const [selectedItem, setSelectedItem] = useState(0);

    const router = useRouter();

    useEffect(() => {
        startAutoPlay();
        return () => stopAutoPlay();
    }, []);

    const startAutoPlay = () => {
        stopAutoPlay();

        intervalRef.current = setInterval(() => {
            iterateItem(1, false);
        }, 2500);
    };

    const stopAutoPlay = () => {
        if (intervalRef.current) {
            clearInterval(intervalRef.current);
            intervalRef.current = null;
        }
    };

    const iterateItem = (delta: number, killTimer = true) => {
        setSelectedItem((prev) => {
            if (prev === undefined) return prev;
            var newVal = prev + delta;

            if (newVal < 0)
                newVal = data.length - 1;

            return newVal % data.length;
        });

        if (killTimer) {
            stopAutoPlay();
        }
    }

    return (<div className="Trending" onClick={() => router.push(`/${data[selectedItem].id}`)}>

        {data[selectedItem].bannerImage != null && <img src={data[selectedItem].bannerImage} />}
        <div className="Trending_ImgOverlay" />

        <div className="Trending_Navigation" onClick={e => e.stopPropagation()}>
            <button onClick={() => iterateItem(-1)}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 256 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                    <path d="M31.7 239l136-136c9.4-9.4 24.6-9.4 33.9 0l22.6 22.6c9.4 9.4 9.4 24.6 0 33.9L127.9 256l96.4 96.4c9.4 9.4 9.4 24.6 0 33.9L201.7 409c-9.4 9.4-24.6 9.4-33.9 0l-136-136c-9.5-9.4-9.5-24.6-.1-34z" />
                </svg>
            </button>
            <a>{`${selectedItem + 1} / ${data.length}`}</a>
            <button onClick={() => iterateItem(1)}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 256 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                    <path d="M224.3 273l-136 136c-9.4 9.4-24.6 9.4-33.9 0l-22.6-22.6c-9.4-9.4-9.4-24.6 0-33.9l96.4-96.4-96.4-96.4c-9.4-9.4-9.4-24.6 0-33.9L54.3 103c9.4-9.4 24.6-9.4 33.9 0l136 136c9.5 9.4 9.5 24.6.1 34z" />
                </svg>
            </button>
        </div>

        <div className="Trending_Info">
            <h1>{data[selectedItem].title}</h1>
            <p dangerouslySetInnerHTML={{ __html: data[selectedItem].description ?? "" }} />
        </div>

        <button className="Trending_PlayButton">
            <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 512 512" height="18" width="18" xmlns="http://www.w3.org/2000/svg">
                <path d="M0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zM188.3 147.1c-7.6 4.2-12.3 12.3-12.3 20.9l0 176c0 8.7 4.7 16.7 12.3 20.9s16.8 4.1 24.3-.5l144-88c7.1-4.4 11.5-12.1 11.5-20.5s-4.4-16.1-11.5-20.5l-144-88c-7.4-4.5-16.7-4.7-24.3-.5z" />
            </svg>
            Watch
        </button>
    </div>)
}