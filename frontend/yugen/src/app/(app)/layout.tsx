import Topbar from "@comps/topbar";

import { ToastProvider } from "../context/toast";
import { ModalProvider } from "../context/modalContext";

import Footer from "../components/footer";
import { UserSessionProvider } from "../context/userContext";

export default function AppLayout({ children }: any) {
    return (
        <div >
            <ToastProvider>
                <ModalProvider>
                    <UserSessionProvider>
                        <Topbar />
                        {children}
                        <Footer />
                    </UserSessionProvider>
                </ModalProvider>
            </ToastProvider>
        </div>
    );
}