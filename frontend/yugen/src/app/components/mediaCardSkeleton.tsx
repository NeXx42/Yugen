import "./mediaCardSkeleton.css"

export default function () {
    return (
        <div className="MediaCardSkeleton">
            <div className="MediaCardSkeleton_Img" />

            <div className="MediaCardSkeleton_Info">
                <h2>Loading</h2>
                <div>
                    <p>Loading</p>
                    <p>Loading</p>
                    <p>Loading</p>
                </div>
            </div>
        </div>
    )
}