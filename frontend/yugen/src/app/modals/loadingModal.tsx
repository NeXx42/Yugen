"use client"

import { useEffect, useState } from "react"
import "./loadingModal.css"

export default function ({ loadingCall, closeRequest }: { loadingCall: () => Promise<any>, closeRequest: () => void }) {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        setError(null);
        setLoading(true);
        loadingCall().then(() => closeRequest()).catch(e => setError(e.message));
    }, [])

    return (<>
        {error != null ? (
            <a>Error - {error}</a>
        ) :
            <a>Loading - {loading}</a>
        }
    </>)
}