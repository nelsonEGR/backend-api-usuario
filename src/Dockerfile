# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurar dependencias primero para aprovechar la cache de capas
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["src/api.csproj", "src/"]
RUN dotnet restore "src/api.csproj"

# Copiar el codigo y publicar
COPY src/ src/
WORKDIR /src/src
RUN dotnet publish "api.csproj" --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true

COPY --from=build /app/publish .

# Ejecutar como usuario sin privilegios (la imagen oficial ya crea "app")
USER app

EXPOSE 8080

ENTRYPOINT ["dotnet", "api.dll"]
