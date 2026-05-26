"use client"

import { createPortal } from "react-dom";
import { createContext, ReactNode, useContext, useEffect, useState } from "react"

import "./modalContext.css"

const ModalContext = createContext<any>(null)

interface ModalRequest {
    content: ReactNode
}

export function ModalProvider({ children }: { children: React.ReactNode }) {
    const [currentModal, setCurrentModal] = useState<ModalRequest | null>(null);
    const [mounted, setMounted] = useState(false);

    const showModal = (content: ReactNode) => {
        setCurrentModal({
            content
        });
    }

    const closeModal = () => {
        setCurrentModal(null);
    }

    useEffect(() => {
        setMounted(true);
    }, []);

    return (
        <ModalContext.Provider value={{ showModal, closeModal }}>
            {children}
            {mounted && currentModal && createPortal((
                <div className="ModalContainer" onClick={() => setCurrentModal(null)}>
                    <div className="ModalContent" onClick={e => e.stopPropagation()}>
                        {currentModal.content}
                    </div>
                </div>
            ), document.body)}
        </ModalContext.Provider>
    )
}

export const useModals = () => useContext(ModalContext)