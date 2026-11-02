export function onRequestError(err: unknown, request: { path: string }) {
    console.error(`[next] ${request.path}`, err);
}