
import * as api from "@lib/api.server"

import "./page.css";

import CardRow from "@/app/components/cardRow";
import TrendingList from "./trendingList";

export default async function () {
    const [upcoming, trending, watching] = await Promise.all([
        api.catalog_Upcoming(),
        api.catalog_Trending(),
        api.library_CurrentWatching(0, 10)
    ])

    return (
        <div className="HomePage" >
            <div>
                <TrendingList data={trending} />
            </div>

            <div>
                <h1 style={{ marginBottom: "5px" }}>Continue Watching</h1>
                <CardRow cards={watching.data} />
            </div>

            <div>
                <h1 style={{ marginBottom: "5px" }}>Continue Watching</h1>
                <CardRow cards={upcoming} />
            </div>
        </div>

    )
}