FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

COPY ["Services/Identity.API/Identity.API.csproj", "Services/Identity.API/"]
COPY ["Services/Identity.Application/Identity.Application.csproj", "Services/Identity.Application/"]
COPY ["Services/Identity.Infrastructure/Identity.Infrastructure.csproj", "Services/Identity.Infrastructure/"]
COPY ["Services/Identity.Domain/Identity.Domain.csproj", "Services/Identity.Domain/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]

RUN --mount=type=secret,id=github_token \
    --mount=type=cache,id=nuget-${TARGETARCH},target=/root/.nuget/packages \
    export GITHUB_TOKEN="$(cat /run/secrets/github_token)" && \
    dotnet restore \
      "Services/Identity.API/Identity.API.csproj" \
      --configfile /src/nuget.config

COPY . .

WORKDIR "/src/Services/Identity.API"

RUN --mount=type=cache,id=nuget-${TARGETARCH},target=/root/.nuget/packages \
    dotnet build "./Identity.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    --no-restore

FROM build AS publish
RUN dotnet publish "./Identity.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final

ARG APP_USER=app
ARG APP_UID=1000

RUN adduser -S -u ${APP_UID} -G ${APP_USER} -h /app ${APP_USER} || true

WORKDIR /app

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=publish --chown=${APP_USER}:${APP_USER} /app/publish .

USER ${APP_USER}
ENTRYPOINT ["dotnet", "Identity.API.dll"]
