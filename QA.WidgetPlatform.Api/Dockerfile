FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["QA.WidgetPlatform.Api/QA.WidgetPlatform.Api.csproj", "QA.WidgetPlatform.Api/"]
COPY nuget.config .
RUN dotnet restore "QA.WidgetPlatform.Api/QA.WidgetPlatform.Api.csproj"
COPY . .
WORKDIR "/src/QA.WidgetPlatform.Api"
RUN dotnet build "QA.WidgetPlatform.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QA.WidgetPlatform.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QA.WidgetPlatform.Api.dll"]