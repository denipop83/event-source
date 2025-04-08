#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["Identifications/Identifications.csproj", "Identifications/"]
#COPY ["Core/Core.csproj", "Core/"]
#COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
#RUN dotnet restore "Identifications/Identifications.csproj"
#COPY . .
#WORKDIR "/src/Identifications"
#RUN dotnet build "Identifications.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "Identifications.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Identifications.dll"]

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["Identifications/Identifications.csproj", "Identifications/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]

COPY . .
WORKDIR "/src/Identifications"

RUN dotnet run -- codegen write
RUN dotnet publish "Identifications.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
ENV DOTNET_RUNNING_IN_CONTAINER=1
ENV DOTNET_NOLOGO=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
RUN addgroup -g 1001 -S nonroot && adduser -u 1001 -S nonroot -G nonroot
RUN mkdir /app
RUN chown nonroot:nonroot /app
WORKDIR /app
COPY --chown=nonroot:nonroot --from=build /app/publish .

FROM runtime
EXPOSE 5000
USER nonroot
ENTRYPOINT ["dotnet", "Application.dll"]