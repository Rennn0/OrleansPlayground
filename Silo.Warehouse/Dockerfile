FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app
COPY Core ./Core
COPY Silo.Warehouse ./Silo.Warehouse
WORKDIR /app/Silo.Warehouse
RUN dotnet restore
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/build ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "Silo.Warehouse.dll"]
