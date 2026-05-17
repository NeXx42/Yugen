import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(req: NextRequest) {
    const token = req.cookies.get("AuthToken");

    if (req.nextUrl.pathname === "/")
        return NextResponse.redirect(new URL('/home', req.url));

    const protectedRoutes = ['/home']

    const isProtectedRoute = protectedRoutes.some((route) =>
        req.nextUrl.pathname == route
    )

    if (isProtectedRoute && !token) {
        return NextResponse.redirect(new URL('/login', req.url))
    }

    return NextResponse.next();
}