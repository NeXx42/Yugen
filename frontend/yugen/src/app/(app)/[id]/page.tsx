import { MediaInfo } from "@/app/shared/types";
import * as api from "@lib/api.server"
import MediaPlayer from "./mediaPlayer";
import EpisodeList from "./episodeList";
import MediaContainer from "./mediaContainer";

export default async function ({ params }: { params: { id: number } }) {
    const { id } = await params;
    const media: MediaInfo = await api.catalog_GetInfo(id);

    void api.media_SyncWatchTime(id);

    return (<div>
        <h1>{media.title}</h1>

        <MediaContainer mediaInfo={media} />
    </div>)
}