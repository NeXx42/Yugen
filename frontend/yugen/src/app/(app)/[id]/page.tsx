import { MediaInfo } from "@/app/shared/types";
import * as api from "@lib/api.server"
import MediaPlayer from "./mediaPlayer";

export default async function ({ params }: { params: { id: string } }) {
    const { id } = await params;

    const media: MediaInfo = await api.catalog_GetInfo(id);

    return (<div>
        <h1>{media.title}</h1>
        <a>{media.isDownloaded ? "test" : "as"}</a>

        <MediaPlayer ItemId="" />
    </div>)
}