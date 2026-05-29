import { Suspense } from "react"
import Page_client from "./page_client";

export default function () {
    return (
        <Suspense fallback={<div>Loading...</div>}>
            <Page_client />
        </Suspense >
    )
}