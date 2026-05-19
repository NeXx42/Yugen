"use client"

import * as api from "@lib/api.local"
import { useState } from "react";

export default function () {
    const [id, setId] = useState("");

    const req = async () => {
        const aniId = Number.parseInt(id);

        await api.library_Request(aniId, "/tv", 1);
    }

    return (<>
        <input value={id} onChange={e => setId(e.target.value)}></input>
        <button onClick={req}>submit</button>
    </>)
}