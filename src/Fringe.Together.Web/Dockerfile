FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Fringe.Together.Web/Fringe.Together.Web.csproj", "Fringe.Together.Web/"]
RUN dotnet restore "Fringe.Together.Web/Fringe.Together.Web.csproj"
COPY . .
WORKDIR "/src/Fringe.Together.Web"
RUN dotnet build "Fringe.Together.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fringe.Together.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fringe.Together.Web.dll"]
