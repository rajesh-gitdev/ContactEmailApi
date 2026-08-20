# syntax=docker/dockerfile:1

# --- Build stage -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Restore first (leverages Docker layer caching for dependencies).
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ContactEmailApi.Domain/*.csproj        src/ContactEmailApi.Domain/
COPY src/ContactEmailApi.Shared/*.csproj        src/ContactEmailApi.Shared/
COPY src/ContactEmailApi.Application/*.csproj    src/ContactEmailApi.Application/
COPY src/ContactEmailApi.Persistence/*.csproj    src/ContactEmailApi.Persistence/
COPY src/ContactEmailApi.Infrastructure/*.csproj src/ContactEmailApi.Infrastructure/
COPY src/ContactEmailApi.Api/*.csproj            src/ContactEmailApi.Api/
RUN dotnet restore src/ContactEmailApi.Api/ContactEmailApi.Api.csproj

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/ContactEmailApi.Api/ContactEmailApi.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Run as the non-root user shipped in the base image.
USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "ContactEmailApi.Api.dll"]
