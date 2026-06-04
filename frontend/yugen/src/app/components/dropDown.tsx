"use client"

import { ReactNode, Ref, useEffect, useImperativeHandle, useRef, useState } from "react";
import "./dropDown.css"

export interface DropDownResults {
    getValue: () => number | undefined
    setValue: (val: number | undefined) => void
}

export default function ({ options, unselected, ref }: { options: string[], unselected: string, ref: Ref<DropDownResults> }) {
    const [open, setOpen] = useState(false);

    const popupRef = useRef<HTMLDivElement | null>(null);
    const searchRef = useRef<HTMLInputElement | null>(null);

    const [search, setSearch] = useState("")
    const [selectedEntry, setSelectedEntry] = useState<number | undefined>(undefined);


    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (popupRef.current && !popupRef.current.contains(event.target as Node)) {
                setOpen(false);
            }
        }

        document.addEventListener("mousedown", handleClickOutside);

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);


    const focusSearch = () => {
        searchRef?.current?.focus();
        setOpen(true);
    }

    const drawOptions = () => {
        const inp = search.toLocaleLowerCase();

        const addSelection = (pos: number | undefined) => {
            setSearch("");
            setSelectedEntry(pos);

            setOpen(false);
        }

        let res: ReactNode[] = [
            <div className={(selectedEntry === undefined) ? "Selected" : ""} key={-1} onClick={() => addSelection(undefined)}>{unselected}</div>
        ]

        for (let i = 0; i < options.length; i++) {
            if (inp === "" || options[i].toLocaleLowerCase().startsWith(inp)) {
                res.push(<div className={i === selectedEntry ? "Selected" : ""} key={i} onClick={() => addSelection(i)}>{options[i]}</div>)
            }
        }

        if (res.length === 0)
            return <p>No options</p>

        return res;
    }

    useImperativeHandle(ref, () => ({
        getValue: () => selectedEntry,
        setValue: (val: number | undefined) => setSelectedEntry(val)
    }), [selectedEntry]);

    return (
        <div ref={popupRef} className="DropDownSearch" >
            <div className="DropDownSearch_Search" onClick={focusSearch} data-selected={open}>
                <div className="DropDownSearch_Container">
                    {
                        ((selectedEntry === undefined) && search === "") ? (
                            <span>{unselected}</span>
                        ) : (
                            <span>{options[selectedEntry!]}</span>
                        )
                    }
                    <input ref={searchRef} onClick={() => setOpen(!open)} type="text" value={search} onChange={e => setSearch(e.target.value)} style={{ width: `${(search?.length ?? 0) + 1}ch` }} />
                </div>
                <svg height="20" width="20" viewBox="0 0 20 20" aria-hidden="true" focusable="false" stroke="currentColor" fill="currentColor" >
                    <path d="M4.516 7.548c0.436-0.446 1.043-0.481 1.576 0l3.908 3.747 3.908-3.747c0.533-0.481 1.141-0.446 1.574 0 0.436 0.445 0.408 1.197 0 1.615-0.406 0.418-4.695 4.502-4.695 4.502-0.217 0.223-0.502 0.335-0.787 0.335s-0.57-0.112-0.789-0.335c0 0-4.287-4.084-4.695-4.502s-0.436-1.17 0-1.615z" />
                </svg>
            </div>

            {open && (
                <div className="DropDownSearch_Options">
                    {drawOptions()}
                </div>
            )}
        </div>
    );
}