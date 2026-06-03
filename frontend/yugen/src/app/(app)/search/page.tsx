import * as api from "@lib/api.server"

import SearchContainer from "./searchContainer";

import "./page.css"
import GenericSearchContainer from "@/app/components/genericSearchContainer";


export default async function ({ searchParams }: { searchParams: { query?: string } }) {
    const { query } = await searchParams;

    const criteria = await api.catalog_SearchCriteria();

    if (query === undefined)
        return <>Please enter a search query</>

    return (<div style={{ marginTop: "35px" }}>
        <GenericSearchContainer criteria={criteria?.data} />
        <SearchContainer searchQuery={query} />
    </div>)
}