# CS_Shop Services (Frontend Guide)

This repo contains three backend services used by the frontend: AuthService, ItemService, and ApiGateway.

## Local URLs (Development)
- AuthService: https://localhost:7135 (HTTP: http://localhost:5079)
- ItemService: https://localhost:7267 (HTTP: http://localhost:5111)
- ApiGateway: https://localhost:7245 (HTTP: http://localhost:5166)

## Auth Flow (Steam)
1) Frontend navigates the user to `GET /auth/login` (preferably via ApiGateway).
2) Steam login redirects back to `GET /auth/callback` in AuthService.
3) AuthService redirects to the frontend with `?token=JWT`.
4) Frontend stores the token and uses it as a Bearer token.

Header for protected endpoints:
```
Authorization: Bearer <JWT>
```

## AuthService Endpoints
Base URL: https://localhost:7135

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

## ItemService Endpoints
Base URL: https://localhost:7267

- `GET /SteamItem/GetSteamItems?steamId=...`
  - Auth required.
  - Response: list of Steam assets.

- `POST /SteamItem/ClearCache?steamId=...`
  - Auth required.
  - Response: status message.

## ApiGateway (Recommended for Frontend)
Base URL: https://localhost:7245

- `GET /auth/login`
- `GET /auth/getuserbyid`
- `GET /SteamItem/{everything}`
- `POST /SteamItem/{everything}`
- `PUT /SteamItem/{everything}`

## Local Configuration
Local secrets live in `AuthService/appsettings.Local.json` and are ignored by git. Fill in:
- `Steam:ApiKey`
- `Jwt:SecretKey`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireDays`
- `ConnectionStrings:DefaultConnection`

## Docker Configuration
Docker secrets live in `.env.local` (ignored by git). Fill in:
- `ConnectionStrings__DefaultConnection`
- `Steam__ApiKey`
- `Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpireDays`
- `JWT_SECRET_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`
- `ASPNETCORE_Kestrel__Certificates__Default__Password`
- `POSTGRES_PASSWORD`
