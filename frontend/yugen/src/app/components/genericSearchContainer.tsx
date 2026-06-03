"use client"

import "./genericSearchContainer.css"

import MultiDropDownSearch, { MultiDropDownResults } from "@/app/components/multiDropDownSearch";
import DropDown, { DropDownResults } from "@/app/components/dropDown";
import { useRef } from "react";
import { SearchCriteria } from "../shared/types";

const status = [
    "Finished",
    "Releasing",
    "Not yet released",
    "Cancelled",
    "Hiatus"
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


export default function ({ criteria }: { criteria: SearchCriteria | null }) {

    const genreRef = useRef<MultiDropDownResults>(null);
    const tagsRef = useRef<MultiDropDownResults>(null);

    const yearsRef = useRef<DropDownResults>(null);
    const statusRef = useRef<DropDownResults>(null);
    const formatRef = useRef<DropDownResults>(null);

    const search = () => {
        console.log(genreRef?.current?.getValue());
        console.log(tagsRef?.current?.getValue());
        console.log(yearsRef?.current?.getValue());
        console.log(statusRef?.current?.getValue());
        console.log(formatRef?.current?.getValue());
    }

    const getYears = (): string[] => {
        const y: string[] = [];
        const year = new Date().getFullYear();

        for (let i = year + 1; i >= 1950; i--)
            y.push(i.toString());

        return y;
    }

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


            <div>
                <button onClick={search}>Search</button>
            </div>
        </div>
    )
}