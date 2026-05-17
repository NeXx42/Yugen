import Topbar from "@comps/topbar";
import { ToastProvider } from "../context/toast";

export default function AppLayout({ children }: any) {
    return (
        <div >
            <ToastProvider>
                <Topbar />
                {children}
            </ToastProvider>
        </div>
    );
}