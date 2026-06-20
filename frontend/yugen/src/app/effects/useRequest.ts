"use client"

import { useCallback, useEffect, useState } from "react";

export function useRequest<T>(
    fn: () => Promise<T>
) {
    const [data, setData] = useState<T | null>(null);
    const [error, setError] = useState<Error | null>(null);
    const [loading, setLoading] = useState(false);


    const execute = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            const result = await fn();
            setData(result);
            return result;
        } catch (err) {
            setError(err as Error);
            throw err;
        } finally {
            setLoading(false);
        }
    }, [fn]);

    return { data, error, loading, execute };
}