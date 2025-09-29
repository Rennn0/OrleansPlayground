# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file(s) and restore dependencies
COPY ["Silo.Warehouse/Silo.Warehouse.csproj", "Silo.Warehouse/"]
RUN dotnet restore "Silo.Warehouse/Silo.Warehouse.csproj"

# Copy the full source and build
COPY . .
WORKDIR "/src/Silo.Warehouse"
RUN dotnet build "./Silo.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Silo.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run the console app
ENTRYPOINT ["dotnet", "Silo.Warehouse.dll"]
