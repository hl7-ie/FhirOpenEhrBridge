# syntax=docker/dockerfile:1

# ---- Build stage ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution + project files first to leverage Docker layer caching on restore.
COPY *.sln ./
COPY src/FhirOpenEhrBridge.Domain/*.csproj src/FhirOpenEhrBridge.Domain/
COPY src/FhirOpenEhrBridge.Application/*.csproj src/FhirOpenEhrBridge.Application/
COPY src/FhirOpenEhrBridge.Infrastructure/*.csproj src/FhirOpenEhrBridge.Infrastructure/
COPY src/FhirOpenEhrBridge.Api/*.csproj src/FhirOpenEhrBridge.Api/
COPY tests/FhirOpenEhrBridge.UnitTests/*.csproj tests/FhirOpenEhrBridge.UnitTests/
COPY tests/FhirOpenEhrBridge.BddTests/*.csproj tests/FhirOpenEhrBridge.BddTests/
RUN dotnet restore src/FhirOpenEhrBridge.Api/FhirOpenEhrBridge.Api.csproj

# Copy the rest of the source and publish a trimmed, release build.
COPY . .
RUN dotnet publish src/FhirOpenEhrBridge.Api/FhirOpenEhrBridge.Api.csproj \
    -c Release -o /app /p:UseAppHost=false

# ---- Runtime stage -------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Run as the non-root user shipped in the aspnet image.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FhirOpenEhrBridge.Api.dll"]
