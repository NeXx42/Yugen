"use client"

import "./genericSearchContainer.css"

import MultiDropDownSearch, { MultiDropDownResults } from "@/app/components/multiDropDownSearch";
import DropDown, { DropDownResults } from "@/app/components/dropDown";
import { Ref, useEffect, useRef } from "react";
import { SearchCriteria, SearchRequest } from "../shared/types";
import { ReadonlyURLSearchParams, useSearchParams } from "next/navigation";

const status = [
    "Finished",
    "Releasing",
    "Not yet released",
    "Cancelled",
    "Hiatus"
]

const statusLookup = [
    "FINISHED",
    "RELEASING",
    "NOT_YET_RELEASED",
    "CANCELLED",
    "HIATUS"
]

const formats = [
    "TV",
    "TV Short",
    "Movie",
    "Special",
    "OVA",
    "ONA",
    "Music"
]

const formatLookup = [
    "TV",
    "TV_SHORT",
    "MOVIE",
    "SPECIAL",
    "OVA",
    "ONA",
    "MUSIC"
]

const getYears = (): string[] => {
    const y: string[] = [];
    const year = new Date().getFullYear();

    for (let i = year + 1; i >= 1950; i--)
        y.push(i.toString());

    return y;
}

const years = getYears();

interface Props {
    criteria: SearchCriteria | null,
    existingQuery?: SearchRequest,
    onSearch: (req: SearchRequest) => void,
}

export function DecodeSearchParams(searchParams: ReadonlyURLSearchParams): SearchRequest {
    const pageTxt: string | null = searchParams.get("page");
    const filter: string | null = searchParams.get("query");

    const format: string | null = searchParams.get("format");
    const status: string | null = searchParams.get("status");
    const year: string | null = searchParams.get("year");

    const genres: string[] | undefined = searchParams.get("genres")?.split(",");
    const tags: string[] | undefined = searchParams.get("tags")?.split(",");

    return {
        pageSize: 0,
        page: Number.isInteger(pageTxt ?? "NotANumber") ? Number.parseInt(pageTxt!) : 1,

        text: filter ?? undefined,

        format: format ?? undefined,
        status: status ?? undefined,
        year: Number.isInteger(year ?? "NotANumber") ? Number.parseInt(year!) : undefined,

        genres: genres ?? undefined,
        tags: tags ?? undefined,
    }
}

export default function ({ criteria, existingQuery, onSearch }: Props) {
    const searchParams = useSearchParams();

    const genreRef = useRef<MultiDropDownResults>(null);
    const tagsRef = useRef<MultiDropDownResults>(null);

    const yearsRef = useRef<DropDownResults>(null);
    const statusRef = useRef<DropDownResults>(null);
    const formatRef = useRef<DropDownResults>(null);

    const search = () => {

        const yearIndex = yearsRef?.current?.getValue();
        const statusIndex = statusRef?.current?.getValue();
        const formatIndex = formatRef?.current?.getValue();

        const genres = genreRef?.current?.getValue();

        const req: SearchRequest = {
            page: 0,
            pageSize: 0,

            year: yearIndex != undefined ? Number.parseInt(years[yearIndex]) : undefined,
            status: statusIndex != undefined ? statusLookup[statusIndex] : undefined,
            format: formatIndex != undefined ? formatLookup[formatIndex] : undefined,

            genres: genres?.map(g => criteria!.genres[g])
        }

        onSearch?.(req);
    }

    useEffect(() => {
        if (existingQuery?.format) formatRef.current?.setValue(formatLookup.indexOf(existingQuery.format));
        if (existingQuery?.status) statusRef.current?.setValue(statusLookup.indexOf(existingQuery.status));
        if (existingQuery?.year) yearsRef.current?.setValue(years.indexOf(existingQuery.year.toString()));

        if (existingQuery?.genres) genreRef.current?.setValue(existingQuery.genres.map(g => criteria!.genres.indexOf(g)));
        if (existingQuery?.tags) genreRef.current?.setValue(existingQuery.tags.map(t => criteria!.tags.indexOf(t)));
    }, [])

    return (
        <div className="Search_Criteria">
            <div>
                <a>Genres</a>
                <MultiDropDownSearch placeholder="Select Genres" options={criteria?.genres ?? []} ref={genreRef} />
            </div>

            <div>
                <a>Tags</a>
                <MultiDropDownSearch placeholder="Select Tags" options={criteria?.tags ?? []} ref={tagsRef} />
            </div>

            <div>
                <a>Years</a>
                <DropDown options={getYears()} unselected="Any years" ref={yearsRef} />
            </div>

            <div>
                <a>Status</a>
                <DropDown options={status} unselected="Any status" ref={statusRef} />
            </div>

            <div>
                <a>Format</a>
                <DropDown options={formats} unselected="Any format" ref={formatRef} />
            </div>

            <div className="Search_Criteria_Controls">
                <a>_</a>
                <button onClick={search}>Apply</button>
            </div>
        </div>
    )
}