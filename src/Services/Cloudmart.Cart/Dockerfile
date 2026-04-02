FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Cloudmart.Cart/Cloudmart.Cart.csproj", "Services/Cloudmart.Cart/"]
RUN dotnet restore "Services/Cloudmart.Cart/Cloudmart.Cart.csproj"
COPY . .
WORKDIR "/src/Services/Cloudmart.Cart"
RUN dotnet build "./Cloudmart.Cart.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cloudmart.Cart.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cloudmart.Cart.dll"]
