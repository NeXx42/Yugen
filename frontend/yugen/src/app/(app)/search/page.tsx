import * as api from "@lib/api.server"

import SearchContainer from "./searchContainer";
import MultiDropDownSearch from "@/app/components/multiDropDownSearch";

import "./page.css"

export default async function ({ searchParams }: { searchParams: { query?: string } }) {
    const { query } = await searchParams;

    const criteria = await api.catalog_SearchCriteria();

    if (query === undefined)
        return <>Please enter a search query</>

    return (<div style={{ marginTop: "35px" }}>
        <div className="Search_Criteria">
            <MultiDropDownSearch title="Genres" placeholder="Select Genres" options={criteria.data?.genres ?? []} />
            <MultiDropDownSearch title="Tags" placeholder="Select Tags" options={criteria.data?.tags.map(t => t.name) ?? []} />
        </div>
        <SearchContainer searchQuery={query} />
    </div>)
}