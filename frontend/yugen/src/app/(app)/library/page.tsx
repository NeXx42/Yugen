import * as api from "@lib/api.server"

import { Suspense } from "react"
import Page_client from "./page_client";

export default async function () {
    const criteria = await api.catalog_SearchCriteria();

    return (
        <Suspense fallback={<div>Loading...</div>}>
            <Page_client criteria={criteria?.data} />
        </Suspense >
    )
}