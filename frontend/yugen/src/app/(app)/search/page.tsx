import * as api from "@lib/api.server"

import SearchContainer from "./searchContainer";

import "./page.css"
import GenericSearchContainer from "@/app/components/genericSearchContainer";
import Results from "./results";


export default async function ({ searchParams }: { searchParams: { query?: string } }) {
    const { query } = await searchParams;

    const criteria = await api.catalog_SearchCriteria();

    return (<div className="SearchPage" style={{ marginTop: "35px" }}>
        <Results criteria={criteria?.data} query={query} />
    </div>)
}