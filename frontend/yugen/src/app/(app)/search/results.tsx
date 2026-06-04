"use client"

import GenericSearchContainer from "@/app/components/genericSearchContainer";
import SearchContainer from "./searchContainer";

import { SearchCriteria, SearchRequest } from "@/app/shared/types";
import { useState } from "react";

export default function ({ criteria, query }: { criteria: SearchCriteria | null, query: string | undefined }) {
    const [searchQuery, setSearchQuery] = useState<SearchRequest>({
        page: 1,
        pageSize: 1,
        text: query
    })

    const onSearch = (req: SearchRequest) => {
        setSearchQuery({
            ...req,
            text: query,
        });
    }

    return (<>
        <GenericSearchContainer criteria={criteria} onSearch={onSearch} />
        <SearchContainer searchQuery={searchQuery} />
    </>
    )
}