FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["QA.WidgetPlatform.Api/QA.WidgetPlatform.Api.csproj", "QA.WidgetPlatform.Api/"]
COPY nuget.config .
RUN dotnet restore "QA.WidgetPlatform.Api/QA.WidgetPlatform.Api.csproj"
COPY . .
WORKDIR "/src/QA.WidgetPlatform.Api"
RUN dotnet publish "QA.WidgetPlatform.Api.csproj" -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_HTTP_URLS="http://+:80"
EXPOSE 80
ENTRYPOINT ["dotnet", "QA.WidgetPlatform.Api.dll"]