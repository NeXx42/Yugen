import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(req: NextRequest) {
    const token = req.cookies.get("AuthCookie");

    const isNotProtected = req.nextUrl.pathname === "/login" || req.nextUrl.pathname.startsWith("/api");

    if (!token && !isNotProtected) {
        console.log("redirect from " + req.nextUrl.pathname);
        //return NextResponse.redirect(new URL("/login", req.url));
    }

    return NextResponse.next();
}