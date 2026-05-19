"use client"

import "./trendingList.css"

import { MediaInfo } from "@/app/shared/types"
import { useEffect, useState } from "react"

import { useRouter } from "next/navigation"

export default function ({ data }: { data: MediaInfo[] }) {
    const [selectedItem, setSelectedItem] = useState(0);
    const router = useRouter();

    useEffect(() => {
        const update = () => {
            setSelectedItem((prev) => {
                if (prev === undefined) return prev;
                return (prev + 1) % data.length;
            });
        };

        const interval = setInterval(update, 2500);
        return () => clearInterval(interval);
    }, []);

    return (<div className="Trending" onClick={() => router.push(`/${data[selectedItem].id}`)}>

        {data[selectedItem].bannerImage != null && <img src={data[selectedItem].bannerImage} />}
        <div className="Trending_ImgOverlay" />

        <div className="Trending_Navigation" onClick={e => e.stopPropagation()}>
            <button onClick={() => setSelectedItem(selectedItem + 1)}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 256 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                    <path d="M31.7 239l136-136c9.4-9.4 24.6-9.4 33.9 0l22.6 22.6c9.4 9.4 9.4 24.6 0 33.9L127.9 256l96.4 96.4c9.4 9.4 9.4 24.6 0 33.9L201.7 409c-9.4 9.4-24.6 9.4-33.9 0l-136-136c-9.5-9.4-9.5-24.6-.1-34z" />
                </svg>
            </button>
            <a>{`${selectedItem + 1} / ${data.length}`}</a>
            <button onClick={() => setSelectedItem(selectedItem + 1)}>
                <svg stroke="currentColor" fill="currentColor" strokeWidth="0" viewBox="0 0 256 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                    <path d="M224.3 273l-136 136c-9.4 9.4-24.6 9.4-33.9 0l-22.6-22.6c-9.4-9.4-9.4-24.6 0-33.9l96.4-96.4-96.4-96.4c-9.4-9.4-9.4-24.6 0-33.9L54.3 103c9.4-9.4 24.6-9.4 33.9 0l136 136c9.5 9.4 9.5 24.6.1 34z" />
                </svg>
            </button>
        </div>

        <div className="Trending_Info">
            <h1>{data[selectedItem].title}</h1>
            <p>{data[selectedItem].description}</p>
        </div>
    </div>)
}