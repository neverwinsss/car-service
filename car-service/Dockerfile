# Образ для хостингов с Docker (Render, Amvera и т.п.).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY car-service.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Папка для SQLite-базы. На платформах с постоянным диском подключите его в /app/App_Data.
RUN mkdir -p /app/App_Data && chown -R $APP_UID /app/App_Data
USER $APP_UID
ENV ASPNETCORE_ENVIRONMENT=Production
# Render передаёт порт в переменной PORT; по умолчанию 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet car-service.dll"]
