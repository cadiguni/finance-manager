FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY FinTrack.sln ./
COPY Directory.Build.props ./
COPY backend/FinTrack.Api/FinTrack.Api.csproj backend/FinTrack.Api/
COPY backend/FinTrack.Application/FinTrack.Application.csproj backend/FinTrack.Application/
COPY backend/FinTrack.Domain/FinTrack.Domain.csproj backend/FinTrack.Domain/
COPY backend/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj backend/FinTrack.Infrastructure/

RUN dotnet restore backend/FinTrack.Api/FinTrack.Api.csproj

COPY backend/ backend/

RUN dotnet publish backend/FinTrack.Api/FinTrack.Api.csproj \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "FinTrack.Api.dll"]
