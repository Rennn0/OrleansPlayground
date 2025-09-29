FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy entire repo
COPY . .

# Restore using the solution
RUN dotnet restore Orleans.sln

# Build the specific project
WORKDIR /src/Silo.Warehouse
RUN dotnet build "Silo.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "Silo.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Silo.Warehouse.dll"]
