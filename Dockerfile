# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + csproj files first (better caching)
COPY NewsletterApp.sln ./
COPY Domain/NewsletterApp.Domain.csproj Domain/
COPY Application/NewsletterApp.Application.csproj Application/
COPY Infrastructure/NewsletterApp.Infrastructure.csproj Infrastructure/
COPY API/NewsletterApp.API.csproj API/

# Restore
RUN dotnet restore API/NewsletterApp.API.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish API/NewsletterApp.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Run ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render provides PORT env var. Bind Kestrel to it.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "NewsletterApp.API.dll"]
