FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AppHost1/AppHost1.csproj", "AppHost1/"]
COPY ["Applicants/Applicants.csproj", "Applicants/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["Identifications/Identifications.csproj", "Identifications/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Queries/Queries.csproj", "Queries/"]
RUN dotnet restore "AppHost1/AppHost1.csproj"
COPY . .
WORKDIR "/src/AppHost1"
RUN dotnet build "AppHost1.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AppHost1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppHost1.dll"]
