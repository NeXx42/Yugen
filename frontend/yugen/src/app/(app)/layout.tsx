import Topbar from "@comps/topbar";

import { ToastProvider } from "../context/toast";
import { ModalProvider } from "../context/modalContext";

import Footer from "../components/footer";

export default function AppLayout({ children }: any) {
    return (
        <div >
            <ToastProvider>
                <ModalProvider>
                    <Topbar />
                    {children}
                    <Footer />
                </ModalProvider>
            </ToastProvider>
        </div>
    );
}