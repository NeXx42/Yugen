"use client"

import { ReactNode, Ref, useEffect, useImperativeHandle, useRef, useState } from "react";
import "./multiDropDownSearch.css"


export interface MultiDropDownResults {
    getValue: () => number[]
    setValue: (val: number[]) => void;
}

export default function ({ options, placeholder, ref }: { options: string[], placeholder: string, ref: Ref<MultiDropDownResults> }) {
    const [open, setOpen] = useState(false);

    const popupRef = useRef<HTMLDivElement | null>(null);
    const searchRef = useRef<HTMLInputElement | null>(null);

    const [search, setSearch] = useState("")
    const [selectedEntries, setSelectedEntries] = useState<number[]>([]);


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

        const addSelection = (pos: number) => {
            setSearch("");
            setSelectedEntries(prev => {
                if (prev === undefined) return prev;
                return [
                    ...prev,
                    pos
                ]
            })

            focusSearch();
        }

        let res: ReactNode[] = []

        for (let i = 0; i < options.length; i++) {
            if (inp === "" || options[i].toLocaleLowerCase().startsWith(inp)) {
                if (selectedEntries!.indexOf(i) === -1) {
                    res.push(<div key={i} onClick={() => addSelection(i)}>{options[i]}</div>)
                }
            }
        }

        if (res.length === 0)
            return <p>No options</p>

        return res;
    }

    const removeSelection = (pos: number) => {
        setSelectedEntries(prev => {
            if (prev === undefined) return prev;
            return prev.filter(p => p !== pos);
        })
    }

    useImperativeHandle(ref, () => ({
        getValue: () => selectedEntries,
        setValue: (val: number[]) => {
            setSelectedEntries(val.filter(v => v >= 0));
        }
    }), [selectedEntries]);

    return (
        <div ref={popupRef} className="MultiDropDownSearch" >
            <div className="MultiDropDownSearch_Search" onClick={focusSearch} data-selected={open}>
                <div className="MultiDropDownSearch_Container">
                    {
                        ((selectedEntries?.length ?? 0) == 0 && search === "") ? (
                            <span>{placeholder}</span>
                        ) : (
                            selectedEntries.map(o =>
                                <div key={o}>
                                    <p>
                                        {options[o]}
                                    </p>
                                    <div onClick={() => removeSelection(o)}>
                                        <svg height="14" width="14" viewBox="0 0 20 20" aria-hidden="true" focusable="false" stroke="currentColor" fill="currentColor">
                                            <path d="M14.348 14.849c-0.469 0.469-1.229 0.469-1.697 0l-2.651-3.030-2.651 3.029c-0.469 0.469-1.229 0.469-1.697 0-0.469-0.469-0.469-1.229 0-1.697l2.758-3.15-2.759-3.152c-0.469-0.469-0.469-1.228 0-1.697s1.228-0.469 1.697 0l2.652 3.031 2.651-3.031c0.469-0.469 1.228-0.469 1.697 0s0.469 1.229 0 1.697l-2.758 3.152 2.758 3.15c0.469 0.469 0.469 1.229 0 1.698z" />
                                        </svg>
                                    </div>
                                </div>)
                        )
                    }
                    <input ref={searchRef} onClick={() => setOpen(!open)} type="text" value={search} onChange={e => setSearch(e.target.value)} style={{ width: `${(search?.length ?? 0) + 1}ch` }} />
                </div>
                <svg height="20" width="20" viewBox="0 0 20 20" aria-hidden="true" focusable="false" stroke="currentColor" fill="currentColor" >
                    <path d="M4.516 7.548c0.436-0.446 1.043-0.481 1.576 0l3.908 3.747 3.908-3.747c0.533-0.481 1.141-0.446 1.574 0 0.436 0.445 0.408 1.197 0 1.615-0.406 0.418-4.695 4.502-4.695 4.502-0.217 0.223-0.502 0.335-0.787 0.335s-0.57-0.112-0.789-0.335c0 0-4.287-4.084-4.695-4.502s-0.436-1.17 0-1.615z" />
                </svg>
            </div>

            {open && (
                <div className="MultiDropDownSearch_Options">
                    {drawOptions()}
                </div>
            )}
        </div>
    );
}