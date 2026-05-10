# ── Build Stage ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file
COPY *.sln .

# Copy project files
COPY src/Optrixa.Domain/Optrixa.Domain.csproj \
     src/Optrixa.Domain/
COPY src/Optrixa.Application/Optrixa.Application.csproj \
     src/Optrixa.Application/
COPY src/Optrixa.Infrastructure/Optrixa.Infrastructure.csproj \
     src/Optrixa.Infrastructure/
COPY src/Optrixa.API/Optrixa.API.csproj \
     src/Optrixa.API/

# Restore packages
RUN dotnet restore src/Optrixa.API/Optrixa.API.csproj

# Copy source code
COPY src/ src/

# Publish release build
RUN dotnet publish src/Optrixa.API/Optrixa.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime Stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Render uses port 10000
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Optrixa.API.dll"]