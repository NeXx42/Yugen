import { MediaCardInfo } from "../shared/types";
import MediaCard from "./mediaCard";

import "./cardRow.css"

interface Props {
    cards: MediaCardInfo[],
    viewMoreLink?: string
}

export default function (props: Props) {
    return (
        <div className="CardRow_Cards">
            {props.cards?.map(x => <MediaCard Card={x} key={x.aniListId} />)}
            {
                props.viewMoreLink && (
                    <a href={props.viewMoreLink} className="CardRow_Cards_ViewMore">
                        View More
                    </a>
                )
            }
        </div>
    )
}