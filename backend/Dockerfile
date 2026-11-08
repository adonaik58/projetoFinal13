# syntax=docker/dockerfile:1.7
# ---- build ------------------------------------------------------------------
# Build natively on the CI/host architecture and cross-compile (ReadyToRun) for the target image architecture.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH
WORKDIR /src

# Restore first (layer cached until a project file or the package versions change)
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/Parking.Domain/Parking.Domain.csproj src/Parking.Domain/
COPY src/Parking.Application/Parking.Application.csproj src/Parking.Application/
COPY src/Parking.Infrastructure/Parking.Infrastructure.csproj src/Parking.Infrastructure/
COPY src/Parking.Api/Parking.Api.csproj src/Parking.Api/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/Parking.Api/Parking.Api.csproj -a $TARGETARCH -p:PublishReadyToRun=true

COPY src/ src/
ARG VERSION=0.0.0-local
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/Parking.Api/Parking.Api.csproj -c Release -o /app --no-restore -a $TARGETARCH --self-contained false \
      -p:PublishReadyToRun=true -p:UseAppHost=false -p:InformationalVersion=${VERSION}

# ---- runtime ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
# ICU for culture-aware formatting, tzdata for the business time zone (Africa/Luanda)
RUN apk add --no-cache icu-libs tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=1 \
    DOTNET_GCHeapAffinitizeMask=0 \
    TZ=UTC
WORKDIR /app
COPY --from=build --chown=root:root /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Parking.Api.dll"]
