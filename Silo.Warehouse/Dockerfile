FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app
COPY Core ./Core
COPY Silo.Warehouse ./Silo.Warehouse
RUN dotnet restore Silo.Warehouse/Silo.Warehouse.csproj
RUN dotnet build Silo.Warehouse/Silo.Warehouse.csproj -c $BUILD_CONFIGURATION -o /app/build
EXPOSE 5000
ENTRYPOINT ["dotnet", "build/Silo.Warehouse.dll"]
