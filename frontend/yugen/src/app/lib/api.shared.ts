import { SERVER_URL } from "@shared/config";

const URL = typeof window === "undefined"
    ? SERVER_URL
    : "";

export async function get<T>(uri: string, nextCaching: NextFetchRequestConfig | undefined = undefined, authToken: string | undefined = undefined): Promise<T | undefined> {
    const res = await fetch(`${URL}/api/${uri}`, {
        method: "GET",
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
            ...(authToken ? { Cookie: `AuthToken=${authToken}` } : {})
        },
        next: nextCaching
    });

    if (!res.ok) {
        throw await handleException(res);
    }

    if (res.status === 204 || res.headers.get("content-length") === "0") {
        return undefined;
    }

    return res.json();
}

export async function getPlain(uri: string) {
    const res = await fetch(`${URL}/api/${uri}`, {
        method: "GET",
        credentials: "include",
    });

    if (!res.ok) {
        throw await handleException(res);
    }

    return res;
}

export async function post<T>(uri: string, obj?: any, nextCaching: NextFetchRequestConfig | undefined = undefined, authToken: string | undefined = undefined): Promise<T | undefined> {
    const res = await fetch(`${URL}/api/${uri}`, {
        method: "POST",
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
            ...(authToken ? { Cookie: `AuthToken=${authToken}` } : {})
        },
        body: obj ? JSON.stringify(obj) : "",
        next: nextCaching
    });

    if (!res.ok) {
        throw await handleException(res);
    }

    if (res.status === 204 || res.headers.get("content-length") === "0") {
        return undefined;
    }

    return res.json();
}

export async function upload<T>(uri: string, obj: FormData): Promise<T | undefined> {
    const res = await fetch(`${URL}/api/${uri}`, {
        method: "POST",
        credentials: "include",
        body: obj,
    });

    if (!res.ok) {
        throw await handleException(res);
    }

    if (res.status === 204 || res.headers.get("content-length") === "0") {
        return undefined;
    }

    return res.json();
}

export async function deleteReq<T>(uri: string): Promise<T | undefined> {
    const res = await fetch(`${URL}/api/${uri}`, {
        method: "DELETE",
        credentials: "include",
    });

    if (!res.ok) {
        throw await handleException(res);
    }

    if (res.status === 204 || res.headers.get("content-length") === "0") {
        return undefined;
    }

    return res.json();
}

async function handleException(res: Response): Promise<Error> {
    try {
        const errorData = await res.json();
        return new Error(errorData.message || JSON.stringify(errorData));
    } catch (_) {
        return new Error(`Request failed with status ${res.status}`)
    }
}