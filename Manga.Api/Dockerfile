FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Manga.Api/Manga.Api.csproj", "Manga.Api/"]
COPY ["Manga.Service/Manga.Service.csproj", "Manga.Service/"]
COPY ["Manga.Repository/Manga.Repository.csproj", "Manga.Repository/"]
RUN dotnet restore "Manga.Api/Manga.Api.csproj"
COPY . .
WORKDIR "/src/Manga.Api"
RUN dotnet build "./Manga.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Manga.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Manga.Api.dll"]
