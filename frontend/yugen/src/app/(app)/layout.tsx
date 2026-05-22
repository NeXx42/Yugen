import Topbar from "@comps/topbar";
import { ToastProvider } from "../context/toast";
import Footer from "../components/footer";

export default function AppLayout({ children }: any) {
    return (
        <div >
            <ToastProvider>
                <Topbar />
                {children}
                <Footer />
            </ToastProvider>
        </div>
    );
}