
import * as api from "@lib/api.server"

import "./page.css";

import CardRow from "@/app/components/cardRow";
import TrendingList from "./trendingList";
import SearchList from "./searchList";
import MediaCard from "@/app/components/mediaCard";
import MediaCardHorizontal from "@/app/components/mediaCardHorizontal";

export default async function () {
    const [trending, upcoming, watching, searchCriteria] = await Promise.all([
        api.catalog_Trending(),
        api.catalog_Upcoming(),
        api.library_CurrentWatching(0, 10),
        api.catalog_SearchCriteria()
    ])

    return (
        <div className="HomePage" >
            <div className="HomePage_Trending">
                <TrendingList data={trending} />

                <div className="HomePage_Trending_Genres">
                    <div className="HomePage_Trending_Genres_Scroll">
                        {searchCriteria.genres.map(g => (<a>{g}</a>))}
                    </div>
                </div>
            </div>

            <div>
                <h1 style={{ marginBottom: "5px" }}>Continue Watching</h1>
                <CardRow cards={watching.data.sort((a, b) => (b.watchLastTime ?? 0) - (a.watchLastTime ?? 0))} />
            </div>

            <div className="HomePage_Body">
                <SearchList />

                <div className="HomePage_Right">
                    <div className="HomePage_UpcomingEpisodes">
                        <h2>Airing</h2>
                        {upcoming.slice(0, 5).map(u => <MediaCardHorizontal Card={u} />)}
                    </div>

                    <div className="HomePage_UpcomingEpisodes">
                        <h2>Upcoming</h2>
                        {upcoming.slice(0, 5).map(u => <MediaCardHorizontal Card={u} />)}
                    </div>
                </div>
            </div>
        </div>

    )
}