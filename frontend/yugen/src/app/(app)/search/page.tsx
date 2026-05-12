import SearchContainer from "./searchContainer";

export default async function ({ searchParams }: { searchParams: { query?: string } }) {
    const { query } = await searchParams

    if (query === undefined)
        return <>Please enter a search query</>

    return (<div>
        <h1>SEARCH</h1>
        <SearchContainer searchQuery={query} />
    </div>)
}