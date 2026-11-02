import "./errorBox.css"

interface Props {
    error: any,
    callback?: () => void
}

export default function (props: Props) {
    return (
        <div className="ErrorBox" >
            <div className="ErrorBox_Container">
                {"Failed to load"}
                {props.callback && <button onClick={() => props.callback!()}>Retry</button>}
            </div>
        </div>)
}