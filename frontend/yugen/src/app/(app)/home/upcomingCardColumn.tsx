"use client"
import * as api from "@lib/api.local"

import { MediaCardInfo } from "@/app/shared/types";
import { useRequest } from "@/app/effects/useRequest";
import CardColumn from "../../components/cardColumn";
import { useEffect } from "react";


export default function () {
    const { data, execute } = useRequest<MediaCardInfo[]>(api.catalog_Upcoming);
    useEffect(() => { execute(); }, [])

    return <CardColumn header="Upcoming" content={data ?? []} />
}