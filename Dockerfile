# syntax=docker/dockerfile:1

FROM node:20-alpine AS frontend

WORKDIR /src

COPY package.json yarn.lock ./
COPY tsconfig.json ./
RUN yarn install --frozen-lockfile --network-timeout 120000

COPY frontend ./frontend
RUN yarn build --env production

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS backend

ARG TARGETARCH

WORKDIR /src

COPY src ./src
COPY Logo ./Logo
COPY --from=frontend /src/_output/UI ./_output/UI

RUN dotnet restore src/NzbDrone.Console/Sonarr.Console.csproj \
    && dotnet restore src/NzbDrone.Mono/Sonarr.Mono.csproj

RUN case "${TARGETARCH}" in \
        amd64) rid=linux-musl-x64 ;; \
        arm64) rid=linux-musl-arm64 ;; \
        *) echo "Unsupported target architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac \
    && dotnet publish src/NzbDrone.Console/Sonarr.Console.csproj \
        --configuration Release \
        --framework net6.0 \
        --runtime "${rid}" \
        --self-contained true \
        -p:RunAnalyzers=false \
        -p:TreatWarningsAsErrors=false \
        --output /out \
    && dotnet publish src/NzbDrone.Mono/Sonarr.Mono.csproj \
        --configuration Release \
        --framework net6.0 \
        --runtime "${rid}" \
        --self-contained true \
        -p:RunAnalyzers=false \
        -p:TreatWarningsAsErrors=false \
        --output /mono-out \
    && cp /mono-out/Sonarr.Mono.dll /out/Sonarr.Mono.dll \
    && cp /mono-out/Mono.Posix.NETStandard.dll /out/Mono.Posix.NETStandard.dll \
    && cp /mono-out/libMonoPosixHelper.so /out/libMonoPosixHelper.so

FROM docker.io/library/alpine:3.24

ARG SONARR_VERSION
ARG PACKAGE_VERSION
ARG PACKAGE_AUTHOR
ARG PACKAGE_BRANCH=develop
ARG PACKAGE_UPDATE_MESSAGE="Update this container by pulling a newer ghcr.io image from this fork."

ENV DOTNET_EnableDiagnostics=0 \
    SONARR__UPDATE__BRANCH=develop

USER root
WORKDIR /app

RUN apk add --no-cache \
        bash \
        ca-certificates \
        catatonit \
        coreutils \
        icu-libs \
        jq \
        libintl \
        nano \
        sqlite-libs \
        tzdata

COPY --from=backend /out /app/bin
COPY --from=frontend /src/_output/UI /app/bin/UI
RUN printf "PackageVersion=%s\\nPackageAuthor=%s\\nPackageGlobalMessage=This image is built from the %s fork.\\nUpdateMethod=Docker\\nUpdateMethodMessage=%s\\nBranch=%s\\nReleaseVersion=%s\\n" \
        "${PACKAGE_VERSION}" "${PACKAGE_AUTHOR}" "${PACKAGE_AUTHOR}" "${PACKAGE_UPDATE_MESSAGE}" "${PACKAGE_BRANCH}" "${SONARR_VERSION}" > /app/package_info \
    && chown -R root:root /app \
    && chmod -R 755 /app \
    && rm -rf /tmp/* /app/bin/Sonarr.Update

COPY entrypoint.sh /entrypoint.sh
RUN chmod 755 /entrypoint.sh

USER nobody:nogroup
WORKDIR /config
VOLUME ["/config"]

EXPOSE 8989

ENTRYPOINT ["/usr/bin/catatonit", "--", "/entrypoint.sh"]
