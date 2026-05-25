"use client"

import * as api from "@lib/api.local"

import "./searchList.css"
import { useEffect, useState } from "react"
import { MediaCardInfo, PageResponse, Season, seasonLookup } from "@shared/types"
import MediaCard from "@/app/components/mediaCard"
import PageContainer from "@/app/components/pageContainer"

type SortType = "New" | "Popular" | "TopRated";
const pageSize = 36;

export default function () {
    const [sort, setSort] = useState<SortType>("New");
    const [page, setPage] = useState<number>(1);

    const [season, setSeason] = useState<Season>("FALL");

    const getFeaturedSeason = (): { season: Season, year: number } => {
        return {
            season: season,
            year: 2026
        };
    }

    const search = (): Promise<PageResponse<MediaCardInfo>> => {
        switch (sort) {
            case "New":
                const { season, year } = getFeaturedSeason();

                return api.catalog_Search({
                    page: page,
                    pageSize: pageSize,

                    sort: 17,

                    year: year,
                    season: season
                });

            default: return api.catalog_Search({
                page: page,
                pageSize: pageSize,

                sort: sort === "Popular" ? 19 : 17
            });
        }
    }

    const drawer = (c: MediaCardInfo) => <MediaCard key={c.aniListId} Card={c} />

    const updateSort = (to: SortType) => {
        setPage(1);
        setSort(to);
    }

    useEffect(() => {
        const currentDate = new Date();
        setSeason(seasonLookup[Math.floor(currentDate.getMonth() / 3)])
    }, [sort])

    return (
        <div className="SearchList">
            <PageContainer pageSize={pageSize} currentPage={page} setCurrentPage={setPage} search={search} drawElement={drawer} track={[sort, season]}>
                <div className="SearchList_Controls">
                    <div className="SearchList_Sort">
                        <button className={sort === "New" ? "Selected" : ""} onClick={() => updateSort("New")}>Newest</button>
                        <button className={sort === "Popular" ? "Selected" : ""} onClick={() => updateSort("Popular")}>Popular</button>
                        <button className={sort === "TopRated" ? "Selected" : ""} onClick={() => updateSort("TopRated")}>Top Rated</button>
                    </div>
                    {
                        sort === "New" && (
                            <div className="SearchList_SubFilter">
                                <button className={season === "WINTER" ? "Selected" : ""} onClick={() => setSeason("WINTER")}>Winter</button>
                                <button className={season === "SPRING" ? "Selected" : ""} onClick={() => setSeason("SPRING")}>Spring</button>
                                <button className={season === "SUMMER" ? "Selected" : ""} onClick={() => setSeason("SUMMER")}>Summer</button>
                                <button className={season === "FALL" ? "Selected" : ""} onClick={() => setSeason("FALL")}>Fall</button>
                            </div>
                        )
                    }
                </div>
            </PageContainer>
        </div >
    )
}