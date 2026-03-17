# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY WongaAuth.sln .
COPY src/WongaAuth.Api/WongaAuth.Api.csproj src/WongaAuth.Api/

# Restore dependencies
RUN dotnet restore src/WongaAuth.Api/WongaAuth.Api.csproj

# Copy remaining source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/WongaAuth.Api/WongaAuth.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WongaAuth.Api.dll"]
