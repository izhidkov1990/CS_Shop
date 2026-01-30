# CS_Shop API (Frontend)

## Local URLs (Development)
- Auth: https://localhost:7135 (HTTP: http://localhost:5079)
- Items: https://localhost:7267 (HTTP: http://localhost:5111)
- Gateway: https://localhost:7245 (HTTP: http://localhost:5166)

## Auth Flow (Steam)
1) Frontend navigates to `GET /auth/login` (preferably via the gateway).
2) Steam login redirects back to `GET /auth/callback`.
3) Auth redirects to the frontend with `?token=JWT`.
4) Frontend stores the token and uses it as a Bearer token.

## Dev Auth Flow (no Steam)
Only in `ASPNETCORE_ENVIRONMENT=Development` with `DevAuth:Enabled=true`.

`POST /auth/dev-login`
- Body (JSON):
  ```json
  {
    "steamId": "76561198000000000",
    "name": "Dev User",
    "email": "dev@example.com",
    "phone": "+1234567890",
    "avatarUrl": "http://avatar.url"
  }
  ```
- Response:
  ```json
  { "token": "JWT" }
  ```
- Optional header if `DevAuth:SharedKey` is set:
  ```
  X-Dev-Auth-Key: <shared-key>
  ```

Header for protected endpoints:
```
Authorization: Bearer <JWT>
```

## API Methods
- `GET /auth/login`
  - Starts Steam OAuth flow.
  - Response: redirect.

- `GET /auth/callback`
  - Steam OAuth callback.
  - Response: redirect to frontend with `token` query param.

- `GET /auth/getuserbyid`
  - Auth required.
  - Response: `UserDTO`.

- `PUT /auth/update_user`
  - Auth required.
  - Body (JSON):
    ```json
    {
      "email": "user@example.com",
      "phone": "+1234567890"
    }
    ```
  - Response: 200 or 404.

- `GET /SteamItem/GetSteamItems?steamId=...`
  - Auth required.
  - Response: list of Steam assets.

- `POST /SteamItem/ClearCache?steamId=...`
  - Auth required.
  - Response: status message.

## Local Configuration
Local secrets live in `config/auth.local.json` and are ignored by git. Fill in:
- `Steam:ApiKey`
- `ConnectionStrings:DefaultConnection`
- `DevAuth:Enabled`, `DevAuth:SharedKey` (dev only)

JWT settings live in `config/local.dev.json`:
- `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireDays`

## Docker Configuration
Docker secrets live in `deploy/.env.local` (ignored by git). Fill in:
- `ConnectionStrings__DefaultConnection`
- `Steam__ApiKey`
- `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpireDays`
- `DevAuth__Enabled`, `DevAuth__SharedKey` (dev only)
- `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`
- `ASPNETCORE_Kestrel__Certificates__Default__Password`
- `POSTGRES_PASSWORD`
