import ContinueWatching from "./continueWatching";
import Upcoming from "./upcoming";

import "./page.css";

export default function () {
    return (
        <div className="HomePage" style={{ margin: "25px" }}>
            <ContinueWatching />
            <Upcoming />
        </div>

    )
}