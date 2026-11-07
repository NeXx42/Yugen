Yugen is a frontend for orchestrating Jellyfin, Sonarr, and Radarr. It is designed specifically for anime. Yugen is intended to serve as the main interface for interacting with and managing these integrated services.
UI is based on https://github.com/Miruro-no-kuon/Miruro

## Features
- Selectable metadata sources (Anilist, Tenari)
- Native HLS & direct playback utilising Jellyfin.
- Watch history & bookmarks
- Anime discovery & notification service

## Preview
### Home
<img width="1920" height="2802" alt="Screenshot 2026-11-07 at 13-22-21 Yugen" src="https://github.com/user-attachments/assets/97758cd0-103e-47ed-9703-b50a3d81fe91" />


### Playback
<img width="1920" height="2068" alt="Screenshot 2026-11-07 at 13-27-29 The Melancholy of Haruhi Suzumiya" src="https://github.com/user-attachments/assets/9b37a72c-d242-4c94-b38a-940436fd15ee" />
<img width="1918" height="981" alt="image" src="https://github.com/user-attachments/assets/aa158040-28aa-4a57-95c8-fe0170bf5daf" />

### Library
<img width="1920" height="985" alt="Screenshot 2026-11-07 at 13-23-52 Yugen" src="https://github.com/user-attachments/assets/65e60a0a-d08b-40b2-908e-eb4e3f155c84" />

### Searching
<img width="1920" height="1134" alt="Screenshot 2026-11-07 at 13-28-32 Yugen" src="https://github.com/user-attachments/assets/d965e201-8c0c-4a0b-b7c9-cbb4dc61da8a" />

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
