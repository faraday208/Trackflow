# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Trackflow.Domain/Trackflow.Domain.csproj Trackflow.Domain/
COPY Trackflow.Application/Trackflow.Application.csproj Trackflow.Application/
COPY Trackflow.Infrastructure/Trackflow.Infrastructure.csproj Trackflow.Infrastructure/
COPY Trackflow.Shared/Trackflow.Shared.csproj Trackflow.Shared/
COPY Trackflow.API/Trackflow.API.csproj Trackflow.API/
RUN dotnet restore Trackflow.API/Trackflow.API.csproj

# Copy all source files and build
COPY Trackflow.Domain/ Trackflow.Domain/
COPY Trackflow.Application/ Trackflow.Application/
COPY Trackflow.Infrastructure/ Trackflow.Infrastructure/
COPY Trackflow.Shared/ Trackflow.Shared/
COPY Trackflow.API/ Trackflow.API/
RUN dotnet publish Trackflow.API/Trackflow.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Trackflow.API.dll"]
