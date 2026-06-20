import { MediaCardInfo } from "../shared/types";
import MediaCard from "./mediaCard";

import "./cardRow.css"
import MediaCardSkeleton from "./mediaCardSkeleton";

interface Props {
    cards: MediaCardInfo[],
    viewMoreLink?: string,
    loading?: boolean,
}

export default function (props: Props) {
    const draw = () => {
        if (props.loading) {
            return (<>
                <MediaCardSkeleton />
                <MediaCardSkeleton />
                <MediaCardSkeleton />
                <MediaCardSkeleton />
                <MediaCardSkeleton />
            </>)
        }

        return (<>
            {props.cards?.map(x => <MediaCard Card={x} key={x.aniListId} />)}
        </>)
    }

    return (
        <div className="CardRow_Cards">
            <div className="CardRow_Cards_Entries">
                {draw()}
            </div>
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