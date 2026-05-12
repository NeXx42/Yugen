"use client"

import * as api from "@lib/api.local"
import CardList from "./cardList";

export default function () {
    return (<CardList fetch={api.catalog_Upcoming()} />)
}