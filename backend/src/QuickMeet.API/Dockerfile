# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/QuickMeet.API/QuickMeet.API.csproj", "src/QuickMeet.API/"]
COPY ["src/QuickMeet.Core/QuickMeet.Core.csproj", "src/QuickMeet.Core/"]
COPY ["src/QuickMeet.Infrastructure/QuickMeet.Infrastructure.csproj", "src/QuickMeet.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "src/QuickMeet.API/QuickMeet.API.csproj"

# Copiar código fuente
COPY . .

# Build
RUN dotnet build "src/QuickMeet.API/QuickMeet.API.csproj" \
    -c Release -o /app/build \
    --no-restore

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "src/QuickMeet.API/QuickMeet.API.csproj" \
    -c Release -o /app/publish \
    --no-build \
    /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar curl para health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copiar binarios publicados
COPY --from=publish /app/publish .

# Configurar variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "QuickMeet.API.dll"]
