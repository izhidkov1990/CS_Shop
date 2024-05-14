
# Base image with ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage for compiling application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AuthService/AuthService.csproj", "AuthService/"]
RUN dotnet restore "./AuthService/./AuthService.csproj"
COPY . .
WORKDIR "/src/AuthService"
RUN dotnet build "./AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage for preparing release artifacts
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Copy SSL certificate
COPY ["AuthService/aspnetapp.pfx", "./"]

# Copy published application from the publish stage
COPY --from=publish /app/publish .

# Set environment variables for Kestrel HTTPS configuration
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="Zx02071990"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="./aspnetapp.pfx"

ENV JWT_SECRET_KEY="!Ghjuhfvvbcn02071990"
ENV JWT_ISSUER="cs_shop"
ENV JWT_AUDIENCE="cs_shop_users"


# Start application
ENTRYPOINT ["dotnet", "AuthService.dll"]
