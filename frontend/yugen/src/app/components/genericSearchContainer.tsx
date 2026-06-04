"use client"

import "./genericSearchContainer.css"

import MultiDropDownSearch, { MultiDropDownResults } from "@/app/components/multiDropDownSearch";
import DropDown, { DropDownResults } from "@/app/components/dropDown";
import { Ref, useEffect, useRef } from "react";
import { SearchCriteria, SearchRequest } from "../shared/types";
import { useSearchParams } from "next/navigation";

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

export default function ({ criteria, onSearch }: { criteria: SearchCriteria | null, onSearch: (req: SearchRequest) => void }) {
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

        const req: SearchRequest = {
            page: 0,
            pageSize: 0,

            year: yearIndex != undefined ? Number.parseInt(years[yearIndex]) : undefined,
            status: statusIndex != undefined ? statusLookup[statusIndex] : undefined,
            format: formatIndex != undefined ? formatLookup[formatIndex] : undefined,
        }

        onSearch?.(req);
    }

    useEffect(() => {
        const format: string | null = searchParams.get("format");
        const status: string | null = searchParams.get("status");
        const year: string | null = searchParams.get("year");

        if (format) formatRef.current?.setValue(formatLookup.indexOf(format));
        if (status) statusRef.current?.setValue(statusLookup.indexOf(status));
        if (year) yearsRef.current?.setValue(years.indexOf(year));
    }, [])

    return (
        <div className="Search_Criteria">
            <div>
                <a>Genres</a>
                <MultiDropDownSearch placeholder="Select Genres" options={criteria?.genres ?? []} ref={genreRef} />
            </div>

            <div>
                <a>Tags</a>
                <MultiDropDownSearch placeholder="Select Tags" options={criteria?.tags.map(t => t.name) ?? []} ref={tagsRef} />
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