FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NewsletterApp.sln ./
COPY Domain/NewsletterApp.Domain.csproj Domain/
COPY Application/NewsletterApp.Application.csproj Application/
COPY Infrastructure/NewsletterApp.Infrastructure.csproj Infrastructure/
COPY API/NewsletterApp.API.csproj API/

RUN dotnet restore API/NewsletterApp.API.csproj

COPY . ./
RUN dotnet publish API/NewsletterApp.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 10000

ENTRYPOINT ["dotnet", "NewsletterApp.API.dll"]
