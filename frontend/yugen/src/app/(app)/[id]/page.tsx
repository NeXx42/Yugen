import * as api from "@lib/api.server"

import { MediaInfo } from "@shared/types";
import MediaContainer from "./mediaContainer";
import CardRow from "@comps/cardRow";

import "./page.css";
import MediaRequester from "./seriesControls";
import MediaCardHorizontal from "@/app/components/mediaCardHorizontal";

export default async function ({ params }: { params: { id: number } }) {
    const { id } = await params;
    const media: MediaInfo = await api.catalog_GetInfo(id);

    void api.media_SyncWatchTime(id);

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

                                <a className="ViewPage_SeriesControl" href={`https://anilist.co/anime/${media.id}`} target="_blank" rel="noopener noreferrer" >
                                    <svg stroke="currentColor" fill="currentColor" role="img" viewBox="0 0 24 24" height="2.4rem" width="2.4rem" xmlns="http://www.w3.org/2000/svg">
                                        <path d="M8.45 15.91H6.067v-5.506h-.028l-1.833 2.454-1.796-2.454H2.39v5.507H0V6.808h2.263l1.943 2.671 1.98-2.671H8.45zm8.499 0h-2.384v-2.883H11.96c.008 1.011.373 1.989.914 2.884l-1.942 1.284c-.52-.793-1.415-2.458-1.415-4.527 0-1.015.211-2.942 1.638-4.37a4.809 4.809 0 0 1 2.737-1.37c.96-.15 1.936-.12 2.905-.12l.555 2.051H15.48c-.776 0-1.389.113-1.839.337-.637.32-1.009.622-1.447 1.78h2.372v-1.84h2.384zm3.922-2.05H24l-.555 2.05h-4.962V6.809h2.388z" />
                                    </svg>
                                </a>
                            </div>
                        </div>
                        <div className="ViewPage_Info_Info">
                            <h2>{media.title}</h2>
                            <p dangerouslySetInnerHTML={{ __html: media.description ?? "" }} />
                        </div>
                    </div>

                </div>

                <div className="ViewPage_Right">
                    {
                        (media.connectedMedia?.length ?? 0) > 1 && (
                            <div className="ViewPage_Seasons ViewPageContainer">
                                <h2>Related</h2>
                                {media.connectedMedia.sort((a, b) => (a.season ?? 0) - (b.season ?? 0)).map(m => <MediaCardHorizontal key={m.card.aniListId} card={m.card} season={m.season} />)}
                            </div>

                        )
                    }
                </div>
            </div>

        </div>
    )
}