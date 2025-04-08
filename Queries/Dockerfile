FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Queries/Queries.csproj", "Queries/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
RUN dotnet restore "Queries/Queries.csproj"
COPY . .
WORKDIR "/src/Queries"
RUN dotnet build "Queries.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Queries.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Queries.dll"]
