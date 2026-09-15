FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BodimedMcpAdapter/BodimedMcpAdapter.csproj", "BodimedMcpAdapter/"]
RUN dotnet restore "BodimedMcpAdapter/BodimedMcpAdapter.csproj"

COPY ["BodimedMcpAdapter/", "BodimedMcpAdapter/"]
RUN dotnet publish "BodimedMcpAdapter/BodimedMcpAdapter.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

USER app
ENTRYPOINT ["dotnet", "BodimedMcpAdapter.dll"]
