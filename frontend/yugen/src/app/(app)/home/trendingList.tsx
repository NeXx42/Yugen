"use client"

import "./trendingList.css"

import * as api from "@lib/api.local"

import { CaughtResponse, MediaInfo, SearchCriteria } from "@/app/shared/types"
import { useEffect, useRef, useState } from "react"

import { useRouter } from "next/navigation"

export default function ({ searchCriteria }: { searchCriteria: CaughtResponse<SearchCriteria> }) {
    const [error, setError] = useState<string | undefined>(undefined);
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState<MediaInfo[] | undefined>();

    const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const [selectedItem, setSelectedItem] = useState(0);

    const router = useRouter();

    useEffect(() => {
        loadContent();
        return () => stopAutoPlay();
    }, [])

    const loadContent = () => {
        setLoading(true);
        setError(undefined);

        api.catalog_Trending()
            .then(setData)
            .then(startAutoPlay)
            .catch(e => setError(e.message))
            .finally(() => setLoading(false))
    }

    const startAutoPlay = () => {
        stopAutoPlay();

        intervalRef.current = setInterval(() => {
            if (data == undefined || loading)
                return;

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
                newVal = data!.length - 1;

            return newVal % data!.length;
        });

        if (killTimer) {
            stopAutoPlay();
        }
    }

    const drawItem = (item: MediaInfo | undefined) => {
        if (error)
            return (<div className="Trending_Error" >
                <div className="Page_Error">
                    {"Failed to load"}
                    <button onClick={() => loadContent()}>Retry</button>
                </div>
            </div>)


        const drawGenres = () => {
            return (
                <div className="Trending_Genres">
                    <button>
                        <svg stroke="currentColor" fill="currentColor" viewBox="0 0 512 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                            <path d="M256 504C119 504 8 393 8 256S119 8 256 8s248 111 248 248-111 248-248 248zM142.1 273l135.5 135.5c9.4 9.4 24.6 9.4 33.9 0l17-17c9.4-9.4 9.4-24.6 0-33.9L226.9 256l101.6-101.6c9.4-9.4 9.4-24.6 0-33.9l-17-17c-9.4-9.4-24.6-9.4-33.9 0L142.1 239c-9.4 9.4-9.4 24.6 0 34z" />
                        </svg>
                    </button>
                    <div className="Trending_Genres_Scroll">
                        {searchCriteria.data?.genres.map(g => (<a key={g} href={`search?genres=${g}`}>{g}</a>))}
                    </div>
                    <button type="button" >
                        <svg stroke="currentColor" fill="currentColor" viewBox="0 0 512 512" height="1em" width="1em" xmlns="http://www.w3.org/2000/svg">
                            <path d="M256 8c137 0 248 111 248 248S393 504 256 504 8 393 8 256 119 8 256 8zm113.9 231L234.4 103.5c-9.4-9.4-24.6-9.4-33.9 0l-17 17c-9.4 9.4-9.4 24.6 0 33.9L285.1 256 183.5 357.6c-9.4 9.4-9.4 24.6 0 33.9l17 17c9.4 9.4 24.6 9.4 33.9 0L369.9 273c9.4-9.4 9.4-24.6 0-34z" />
                        </svg>
                    </button>
                </div>
            )
        }

        return (<>
            <div className="Trending" data-loading={loading} onClick={() => router.push(`/${item?.id}`)}>

                {item?.bannerImage != null && <img src={item.bannerImage} />}
                {
                    !loading && (<>
                        <div className="Trending_ImgOverlay" />

                        <div className="Trending_Navigation" onClick={e => e.stopPropagation()}>
                            <button onClick={() => iterateItem(-1)}>
                                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 256 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M31.7 239l136-136c9.4-9.4 24.6-9.4 33.9 0l22.6 22.6c9.4 9.4 9.4 24.6 0 33.9L127.9 256l96.4 96.4c9.4 9.4 9.4 24.6 0 33.9L201.7 409c-9.4 9.4-24.6 9.4-33.9 0l-136-136c-9.5-9.4-9.5-24.6-.1-34z" />
                                </svg>
                            </button>
                            <a>{`${selectedItem + 1} / ${data?.length}`}</a>
                            <button onClick={() => iterateItem(1)}>
                                <svg stroke="currentColor" fill="currentColor" viewBox="0 0 256 512" height="20" width="20" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M224.3 273l-136 136c-9.4 9.4-24.6 9.4-33.9 0l-22.6-22.6c-9.4-9.4-9.4-24.6 0-33.9l96.4-96.4-96.4-96.4c-9.4-9.4-9.4-24.6 0-33.9L54.3 103c9.4-9.4 24.6-9.4 33.9 0l136 136c9.5 9.4 9.5 24.6.1 34z" />
                                </svg>
                            </button>
                        </div>


                        < div className="Trending_Info">
                            <h1>{item?.title}</h1>
                            <a>{item?.tags.slice(0, 5).map(t => t.title).join(" · ")}</a>
                            <p dangerouslySetInnerHTML={{ __html: item?.description ?? "" }} />
                        </div>

                        <button className="Trending_PlayButton">
                            <svg stroke="currentColor" fill="currentColor" viewBox="0 0 512 512" height="18" width="18" xmlns="http://www.w3.org/2000/svg">
                                <path d="M0 256a256 256 0 1 1 512 0A256 256 0 1 1 0 256zM188.3 147.1c-7.6 4.2-12.3 12.3-12.3 20.9l0 176c0 8.7 4.7 16.7 12.3 20.9s16.8 4.1 24.3-.5l144-88c7.1-4.4 11.5-12.1 11.5-20.5s-4.4-16.1-11.5-20.5l-144-88c-7.4-4.5-16.7-4.7-24.3-.5z" />
                            </svg>
                            Watch
                        </button>
                    </>)
                }
            </div >
            {drawGenres()}
        </>)
    }

    return drawItem(data?.[selectedItem]);
}