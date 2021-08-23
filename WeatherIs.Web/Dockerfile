FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WeatherIs.Web/WeatherIs.Web.csproj", "WeatherIs.Web/"]
RUN dotnet restore "WeatherIs.Web/WeatherIs.Web.csproj"
COPY . .
WORKDIR "/src/WeatherIs.Web"
RUN dotnet build "WeatherIs.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherIs.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherIs.Web.dll"]
