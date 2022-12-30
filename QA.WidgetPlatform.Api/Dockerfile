FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
WORKDIR /src
COPY ["QA.WidgetPlatform.Api/QA.WidgetPlatform.Api.csproj", "QA.WidgetPlatform.Api/"]
COPY nuget.config .
RUN dotnet restore "QA.WidgetPlatform.Api/QA.WidgetPlatform.Api.csproj"
COPY . .
WORKDIR "/src/QA.WidgetPlatform.Api"
RUN dotnet build "QA.WidgetPlatform.Api.csproj" -c Release --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish "QA.WidgetPlatform.Api.csproj" -c Release --no-restore -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QA.WidgetPlatform.Api.dll"]