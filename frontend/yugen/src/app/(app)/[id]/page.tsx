import * as api from "@lib/api.server"

import { MediaInfo, seasonLookup, statusLookup } from "@shared/types";
import MediaContainer from "./mediaContainer";

import MediaRequester from "./seriesControls";
import MediaCardHorizontal from "@/app/components/mediaCardHorizontal";
import CardColumn from "@/app/components/cardColumn";

import "./page.css";
import { Metadata } from "next";

import { cache, ReactNode } from "react";
import MediaRecacher from "./mediaRecacher";

export const getMedia = cache(async (id: number): Promise<MediaInfo | null> => {
    return (await api.catalog_GetInfo(id)).data;
});

export async function generateMetadata({ params }: { params: { id: number } }): Promise<Metadata> {
    const { id } = await params;

    return {
        title: (await getMedia(id))?.title ?? "Anime",
    };
}

export default async function ({ params }: { params: { id: number } }) {
    const { id } = await params;
    const media: MediaInfo | null = await getMedia(id);

    if (media === null) {
        return <>Not found</>
    }

    const getDate = (unixSeconds: number | null): string => {
        if (unixSeconds == null)
            return "Unknown";

        const date = new Date(unixSeconds * 1000);
        const months = [
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        ];

        const day = date.getDate();
        const month = months[date.getMonth()];
        const year = date.getFullYear();

        return `${month} ${day} ${year}`;
    }

    const drawExternalLinks = () => {
        if (!media?.externalLinks)
            return <></>

        let links: ReactNode[] = [];

        if (media.externalLinks["anilist"]) {
            links.push(
                <a key={"anilist"} className="ViewPage_SeriesControl" href={`https://anilist.co/anime/${media.externalLinks["anilist"]}`} target="_blank" rel="noopener noreferrer" >
                    <svg stroke="currentColor" fill="currentColor" role="img" viewBox="0 0 24 24" height="1.2rem" width="1.2rem" xmlns="http://www.w3.org/2000/svg">
                        <path d="M24 17.53v2.421c0 .71-.391 1.101-1.1 1.101h-5l-.057-.165L11.84 3.736c.106-.502.46-.788 1.053-.788h2.422c.71 0 1.1.391 1.1 1.1v12.38H22.9c.71 0 1.1.392 1.1 1.101zM11.034 2.947l6.337 18.104h-4.918l-1.052-3.131H6.019l-1.077 3.131H0L6.361 2.948h4.673zm-.66 10.96-1.69-5.014-1.541 5.015h3.23z" />
                    </svg>
                </a>)
        }

        if (media.externalLinks["mal"]) {
            links.push(
                <a key={"mal"} className="ViewPage_SeriesControl" href={`https://myanimelist.net/anime/${media.externalLinks["mal"]}`} target="_blank" rel="noopener noreferrer" >
                    <svg stroke="currentColor" fill="currentColor" role="img" viewBox="0 0 24 24" height="2.4rem" width="2.4rem" xmlns="http://www.w3.org/2000/svg">
                        <path d="M8.45 15.91H6.067v-5.506h-.028l-1.833 2.454-1.796-2.454H2.39v5.507H0V6.808h2.263l1.943 2.671 1.98-2.671H8.45zm8.499 0h-2.384v-2.883H11.96c.008 1.011.373 1.989.914 2.884l-1.942 1.284c-.52-.793-1.415-2.458-1.415-4.527 0-1.015.211-2.942 1.638-4.37a4.809 4.809 0 0 1 2.737-1.37c.96-.15 1.936-.12 2.905-.12l.555 2.051H15.48c-.776 0-1.389.113-1.839.337-.637.32-1.009.622-1.447 1.78h2.372v-1.84h2.384zm3.922-2.05H24l-.555 2.05h-4.962V6.809h2.388z" />
                    </svg>
                </a>)
        }

        if (links.length === 0)
            return <></>

        return <div style={{ display: "grid", gridTemplateColumns: `repeat(${Object.keys(media?.externalLinks ?? {}).length ?? 0}, 1fr)`, gap: 10 }}>
            {links}
        </div>
    }

    return (
        <div className="ViewPage">
            <MediaContainer mediaInfo={media} />

            <div className="ViewPage_Container">
                <div className="ViewPage_Left" >
                    <div id="ViewPage_EpisodeInfo" />
                    <div className="ViewPage_Info ViewPageContainer">
                        <div>
                            <img src={media.cardImage ?? ""} />
                            <div className="ViewPage_Info_Controls">
                                <MediaRequester mediaInfo={media} />
                                {drawExternalLinks()}
                            </div>
                        </div>
                        <div className="ViewPage_Info_Info">
                            <div className="ViewPage_Info_Info_Title">
                                <h2>{media.title}</h2>
                                <MediaRecacher media={media} />
                            </div>
                            <div className="ViewPage_Info_Info_Tags">
                                {media.genres?.map(t => <a key={t} style={{ backgroundColor: media.colour ?? "#2c1b8c" }} href={`search?genres=${t}`}>{t}</a>)}
                            </div>
                            <p dangerouslySetInnerHTML={{ __html: media.description ?? "" }} />

                            <div className="ViewPage_Info_Info_MetaData">
                                <div>
                                    <div>Format:<strong>{media.type}</strong></div>
                                    <div>Status:<strong>{media.status ? statusLookup[media.status] : "-"}</strong></div>
                                    <div>Episodes:<strong>{media.episodeCount ?? "-"}</strong></div>
                                    <div>Duration:<strong>{media.duration ?? "-"}</strong></div>
                                    <div>Season:<strong>{media.season ? seasonLookup[media.season] : "-"}</strong></div>
                                </div>
                                <div>
                                    <div>Start Date:<strong>{getDate(media.startDate)}</strong></div>
                                    <div>End Date:<strong>{getDate(media.endDate)}</strong></div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

                <div className="ViewPage_Right">
                    {(media.connectedMedia?.length ?? 0) > 1 && (
                        <div className="ViewPage_Seasons ViewPageContainer">
                            <h2>Related</h2>
                            {media.connectedMedia.filter(m => m.card).map(m => <MediaCardHorizontal key={m.card.id} card={m.card} selected={m.card.id === media.id} season={m.season} />)}
                        </div>
                    )}

                    {media.recommended?.length > 0 && <CardColumn content={media.recommended} header="Recommended" limit={5} />}
                </div>
            </div>

        </div>
    )
}