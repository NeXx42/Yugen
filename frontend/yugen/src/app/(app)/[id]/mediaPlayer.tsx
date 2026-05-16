"use client"

import { useEffect, useState } from "react"

import * as api from "@lib/api.local"

import "./mediaPlayer.css"

export default function ({ itemId }: { itemId: string | undefined }) {
    return (
        <div className="MediaPlayer">
            <div className="MediaPlayer_Container">
                {itemId != undefined ? (
                    <iframe
                        src={`https://jellyfin.local/web/index.html#!/details?id=${itemId}`}
                        allow="autoplay; fullscreen"
                    ></iframe>
                ) :
                    (
                        <div className="MediaPlayer_Container_Request">
                            <button>Request</button>
                        </div>
                    )
                }
            </div>
            <div className="MediaPlayer_Controls">

            </div>
        </div>
    )
}