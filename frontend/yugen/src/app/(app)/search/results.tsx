"use client"

import GenericSearchContainer, { DecodeSearchParams } from "@/app/components/genericSearchContainer";
import SearchContainer from "./searchContainer";

import { SearchCriteria, SearchRequest } from "@/app/shared/types";
import { useState } from "react";
import { useSearchParams } from "next/navigation";

export default function ({ criteria, }: { criteria: SearchCriteria | null }) {
    const searchParams = useSearchParams();
    const [searchQuery, setSearchQuery] = useState<SearchRequest>(() => DecodeSearchParams(searchParams))

    return (<>
        <GenericSearchContainer criteria={criteria} existingQuery={searchQuery} onSearch={setSearchQuery} />
        <SearchContainer searchQuery={searchQuery} />
    </>
    )
}