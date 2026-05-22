import * as api from "@lib/api.server"

import SearchContainer from "./searchContainer";

export default async function ({ searchParams }: { searchParams: { query?: string } }) {
    const { query } = await searchParams;

    const criteria = await api.catalog_SearchCriteria();

    if (query === undefined)
        return <>Please enter a search query</>

    return (<div style={{ marginTop: "35px" }}>
        <div className="Search_Criteria">
            <select>
                {criteria.data?.genres.map(g => (<option key={g} value={g}>{g}</option>))}
            </select>

            <select>
                {criteria.data?.tags.map(t => (<option key={t.id} value={t.id}>{t.name}</option>))}
            </select>
        </div>
        <SearchContainer searchQuery={query} />
    </div>)
}