FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
COPY ["Services/Gateway.API/Gateway.API.csproj", "Services/Gateway.API/"]


RUN --mount=type=secret,id=github_token \
    --mount=type=cache,id=nuget-${TARGETARCH},target=/root/.nuget/packages \
    export GITHUB_TOKEN="$(cat /run/secrets/github_token)" && \
    dotnet restore \
      "Services/Gateway.API/Gateway.API.csproj" \
      --configfile /src/nuget.config

COPY . .

WORKDIR "/src/Services/Gateway.API"

RUN --mount=type=cache,id=nuget-${TARGETARCH},target=/root/.nuget/packages \
    dotnet build "./Gateway.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

FROM build AS publish
RUN dotnet publish "./Gateway.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final

ARG APP_USER=app
ARG APP_UID=1000

RUN adduser -S -u ${APP_UID} -G ${APP_USER} -h /app ${APP_USER} || true

WORKDIR /app

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=1 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=publish --chown=${APP_USER}:${APP_USER} /app/publish .

USER ${APP_USER}
ENTRYPOINT ["dotnet", "Gateway.API.dll"]
