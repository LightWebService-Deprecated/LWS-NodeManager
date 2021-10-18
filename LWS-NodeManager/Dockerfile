FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["LWS-NodeManager/LWS-NodeManager.csproj", "LWS-NodeManager/"]
RUN dotnet restore "LWS-NodeManager/LWS-NodeManager.csproj"
COPY . .
WORKDIR "/src/LWS-NodeManager"
RUN dotnet build "LWS-NodeManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LWS-NodeManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LWS-NodeManager.dll"]
