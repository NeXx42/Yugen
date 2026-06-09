# frontend build
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

COPY frontend/yugen/package.json frontend/yugen/package-lock.json ./
RUN npm ci

COPY frontend/yugen/public ./public
COPY frontend/yugen/src ./src

COPY frontend/yugen/*.ts ./
COPY frontend/yugen/*.mjs ./
COPY frontend/yugen/tsconfig.json ./

COPY frontend/yugen/.env.docker .env

RUN npm run build


# backend build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build

WORKDIR /src

COPY backend/Yugen.Api/Yugen.Api.csproj ./backend/Yugen.Api/
RUN dotnet restore ./backend/Yugen.Api/Yugen.Api.csproj

COPY backend/. ./backend/

WORKDIR /src/backend/Yugen.Api
RUN dotnet publish -c Release -o /app/publish

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# install ONLY what you actually need (optional, keep minimal)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# backend
COPY --from=backend-build /app/publish ./backend

# frontend
COPY --from=frontend-build /app/frontend/.next/standalone ./frontend
COPY --from=frontend-build /app/frontend/.next/static ./frontend/.next/static
COPY --from=frontend-build /app/frontend/public ./frontend/public

EXPOSE 3000

ARG GIT_COMMIT=unknown
ENV GIT_COMMIT=$GIT_COMMIT

ARG BUILD_NUMBER=0
ENV BUILD_NUMBER=$BUILD_NUMBER

# start script
COPY start.sh /start.sh
RUN chmod +x /start.sh

CMD ["/start.sh"]