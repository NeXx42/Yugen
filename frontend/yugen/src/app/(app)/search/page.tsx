import SearchContainer from "./searchContainer";

export default async function ({ searchParams }: { searchParams: { query?: string } }) {
    const { query } = await searchParams

    if (query === undefined)
        return <>Please enter a search query</>

    return (<div style={{ marginTop: "35px" }}>
        <SearchContainer searchQuery={query} />
    </div>)
}