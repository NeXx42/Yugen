import type { NextConfig } from "next";

const NEXT_PUBLIC_API_URL = process.env.NEXT_PUBLIC_API_URL;

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${NEXT_PUBLIC_API_URL}/api/:path*`
      }
    ];
  },
  // 👇 FORCE legacy webpack mode
  webpack(config) {
    return config
  },

  turbopack: {}, // 👈 required to silence Next 16 error
};

export default nextConfig;
