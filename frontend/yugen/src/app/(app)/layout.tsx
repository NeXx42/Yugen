import Topbar from "@comps/topbar";

export default function AppLayout({ children }: any) {
    return (
        <div >
            <Topbar />
            {children}
        </div>
    );
}