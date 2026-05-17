import * as api from "@lib/api.server"

import { User } from "@shared/types"
import "./loginForm"
import LoginForm from "./loginForm";

export const dynamic = "force-dynamic";

export default async function () {

    const usrs: User[] = await api.getAllUsers();

    return (
        <>
            <h1>Login</h1>
            <LoginForm usrs={usrs} />
        </>
    )
}