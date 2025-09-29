FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./Silo.Warehouse.csproj", "Silo.Warehouse/"]
RUN dotnet restore "Silo.Warehouse/Silo.Warehouse.csproj"
COPY . .
WORKDIR "/src/Silo.Warehouse"
RUN dotnet build "./Silo.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Silo.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Silo.Warehouse.dll"]
