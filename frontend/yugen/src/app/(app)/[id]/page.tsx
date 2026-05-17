import * as api from "@lib/api.server"

import { MediaInfo } from "@shared/types";
import MediaContainer from "./mediaContainer";
import CardRow from "@comps/cardRow";

import "./page.css";

export default async function ({ params }: { params: { id: number } }) {
    const { id } = await params;
    const media: MediaInfo = await api.catalog_GetInfo(id);

    void api.media_SyncWatchTime(id);

    console.log(media);

    return (
        <div className="ViewPage">


            <MediaContainer mediaInfo={media} />

            <div className="ViewPage_Info ViewPageContainer">
                <img src={media.cardImage ?? ""} />
                <div className="ViewPage_Info_Info">
                    <h2>{media.title}</h2>
                </div>
            </div>

            {
                (media.connectedMedia?.length ?? 0) > 1 && (
                    <div className="ViewPage_Seasons ViewPageContainer">
                        <CardRow cards={media.connectedMedia?.sort(m => m.season ?? 0).map(m => m.card)} />
                    </div>
                )
            }
        </div>
    )
}