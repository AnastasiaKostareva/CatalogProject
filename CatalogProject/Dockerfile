FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CatalogProject/CatalogProject.csproj", "CatalogProject/"]
RUN dotnet restore "CatalogProject/CatalogProject.csproj"
COPY . .
WORKDIR "/src/CatalogProject"
RUN dotnet build "./CatalogProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CatalogProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM build AS bundle-build
ARG BUILD_CONFIGURATION=Release
ENV CONNECTION_STRING="Host=placeholder;Database=placeholder;Username=placeholder;Password=placeholder"
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project CatalogProject.csproj \
    --configuration $BUILD_CONFIGURATION \
    --output /app/efbundle \
    --self-contained -r linux-x64

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrator
WORKDIR /app
COPY --from=bundle-build /app/efbundle .
ENTRYPOINT ["./efbundle"]


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogProject.dll"]
