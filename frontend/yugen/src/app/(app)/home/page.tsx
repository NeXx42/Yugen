
import * as api from "@lib/api.server"

import "./page.css";

import TrendingList from "./trendingList";
import SearchList from "./searchList";
import UpcomingCardColumn from "./upcomingCardColumn";
import WatchHistory from "./watchHistory";

export default async function () {
    const [searchCriteria] = await Promise.all([
        api.catalog_SearchCriteria()
    ])

    return (
        <div className="HomePage" >
            <div className="HomePage_Trending">
                <TrendingList searchCriteria={searchCriteria} />
            </div>

            <WatchHistory />

            <div className="HomePage_Body">
                <SearchList />

                <div className="HomePage_Right">
                    <UpcomingCardColumn />
                </div>
            </div>
        </div>

    )
}