#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ItemService/ItemService.csproj", "ItemService/"]
RUN dotnet restore "./ItemService/./ItemService.csproj"
COPY . .
WORKDIR "/src/ItemService"
RUN dotnet build "./ItemService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ItemService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy SSL certificate
COPY ["AuthService/aspnetapp.pfx", "./"]

# Set environment variables for Kestrel HTTPS configuration
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="Zx02071990"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="./aspnetapp.pfx"

ENV JWT_SECRET_KEY="!Ghjuhfvvbcn02071990"
ENV JWT_ISSUER="cs_shop"
ENV JWT_AUDIENCE="cs_shop_users"

ENTRYPOINT ["dotnet", "ItemService.dll"]