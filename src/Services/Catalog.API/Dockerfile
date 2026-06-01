FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Cloudmart.Catalog/Cloudmart.Catalog.csproj", "Services/Cloudmart.Catalog/"]
RUN dotnet restore "Services/Cloudmart.Catalog/Cloudmart.Catalog.csproj"
COPY . .
WORKDIR "/src/Services/Cloudmart.Catalog"
RUN dotnet build "./Cloudmart.Catalog.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cloudmart.Catalog.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cloudmart.Catalog.dll"]
