
import * as api from "@lib/api.server"

import "./page.css";

import CardRow from "@/app/components/cardRow";
import TrendingList from "./trendingList";
import SearchList from "./searchList";
import MediaCardHorizontal from "@/app/components/mediaCardHorizontal";
import CardColumn from "@/app/components/cardColumn";

export default async function () {
    const [upcoming, watching, searchCriteria] = await Promise.all([
        api.catalog_Upcoming(),
        api.library_CurrentWatching(0, 10),
        api.catalog_SearchCriteria()
    ])
    console.log(watching.data?.data);
    return (
        <div className="HomePage" >
            <div className="HomePage_Trending">
                <TrendingList searchCriteria={searchCriteria} />
            </div>

            <div>
                <h1 style={{ marginBottom: "5px" }}>Continue Watching</h1>
                <CardRow cards={watching.data?.data.sort((a, b) => (b.watchLastTime ?? 0) - (a.watchLastTime ?? 0)).slice(0, 7) ?? []} />
            </div>

            <div className="HomePage_Body">
                <SearchList />

                <div className="HomePage_Right">
                    <CardColumn content={upcoming?.data ?? []} limit={10} header="Upcoming" />
                </div>
            </div>
        </div>

    )
}