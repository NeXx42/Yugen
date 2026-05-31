Yugen is a frontend for orchestrating Jellyfin, Sonarr, and Radarr. It is designed specifically for anime, using AniList as its primary metadata provider. Yugen is intended to serve as the main interface for interacting with and managing these integrated services.
UI is based on https://github.com/Miruro-no-kuon/Miruro

## Features
- Anilist metadata provision
- Native HLS & direct playback utilising Jellyfin.
- Watch history & bookmarks
- Anime discovery & notification service

## Preview
### Home
<img width="1918" height="982" alt="image" src="https://github.com/user-attachments/assets/62644a4b-9a69-48b0-9a55-a698069ec7b4" />
<img width="1920" height="956" alt="image" src="https://github.com/user-attachments/assets/352c5955-1de5-45dd-a0a2-2fedb54eee57" />

### Playback
<img width="1923" height="981" alt="image" src="https://github.com/user-attachments/assets/ac9b343c-e9a4-4256-a0f0-54311d50a437" />
<img width="1920" height="983" alt="image" src="https://github.com/user-attachments/assets/cd38ac77-2b4f-403c-b30e-85455bba3d66" />
<img width="1918" height="981" alt="image" src="https://github.com/user-attachments/assets/aa158040-28aa-4a57-95c8-fe0170bf5daf" />

### Library
<img width="1919" height="982" alt="image" src="https://github.com/user-attachments/assets/9332d205-83de-4cd1-b920-906ff81aae52" />

## Installation 

Can be used with Docker Compose. Requires two services, the postgres database and the application. This is an example compose file.

> [!IMPORTANT]
> You must provide a proper Encryption__JWTToken. You can generate a token in linux using ```sh openssl rand -base64 64```

```yml
yugen_db:
    image: postgres:16
    container_name: yugen_db
    restart: always
    environment:
      POSTGRES_USER: yugen
      POSTGRES_PASSWORD: yugen
      POSTGRES_DB: yugen
    ports:
      - "5432:5432"
    volumes:
      - ./yugen:/var/lib/postgresql/data

yugen:
    image: ghcr.io/nexx42/yugen
    container_name: yugen
    restart: always
    depends_on:
      - yugen_db
    environment:
      ConnectionStrings__DefaultConnection: Host=yugen_db;Port=5432;Database=yugen;Username=usr;Password=pass
      Encryption__JWTToken: "BASE64STRING"
    ports:
      - "3000:3000"
```
