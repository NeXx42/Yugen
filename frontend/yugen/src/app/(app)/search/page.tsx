import * as api from "@lib/api.server"

import "./page.css"
import Results from "./results";

export default async function () {
    const criteria = await api.catalog_SearchCriteria();

    return (<div className="SearchPage" style={{ marginTop: "35px" }}>
        <Results criteria={criteria?.data} />
    </div>)
}