"use client"

import * as api from "@lib/api.local"

import { createContext, useContext, useEffect, useState } from "react";
import { UserSession } from "../shared/types";


interface UserSessionProviderProps {
    session: UserSession | undefined;
    login: (username: string, password: string) => Promise<boolean>
    logout: () => Promise<void>

    updateSessionSettings: (settings: Record<string, string>) => void;
}

const UserContext = createContext<UserSessionProviderProps>({
    session: undefined,
    login: (_, __) => Promise.resolve(false),
    logout: () => Promise.resolve(),

    updateSessionSettings: (settings: Record<string, string>) => { }
});

export function UserSessionProvider({ children }: { children: React.ReactNode }) {
    const [session, setSession] = useState<UserSession | undefined>(undefined);

    useEffect(() => {
        api.getSession()
            .catch(e => {
                console.log(e)
                return undefined;
            })
            .then(setSession);
    }, [])

    async function logout() {
        await api.auth_Logout();
        setSession(undefined);
    }

    async function login(username: string, password: string): Promise<boolean> {

        try {
            const res = await api.auth_Login(username, password);

            if (!res)
                return false;
        }
        catch {
            return false;
        }

        return true;
    }

    function updateClientSideSettings(settings: Record<string, string>) {
        if (!session)
            return;

        setSession(prev => ({
            ...prev!,
            settings
        }))
    }

    return (
        <UserContext.Provider value={{ session, login, logout, updateSessionSettings: updateClientSideSettings }}>
            {children}
        </UserContext.Provider>
    )
}

export const useUser = () => useContext(UserContext);