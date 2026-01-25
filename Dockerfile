FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SmartPortfolio.API/SmartPortfolio.API.csproj", "SmartPortfolio.API/"]
COPY ["SmartPortfolio.Domain/SmartPortfolio.Domain.csproj", "SmartPortfolio.Domain/"]
COPY ["SmartPortfolio.Infrastructure/SmartPortfolio.Infrastructure.csproj", "SmartPortfolio.Infrastructure/"]
COPY ["SmartPortfolio.Application/SmartPortfolio.Application.csproj", "SmartPortfolio.Application/"]

RUN dotnet restore "SmartPortfolio.API/SmartPortfolio.API.csproj"

COPY . .

WORKDIR "/src/SmartPortfolio.API"
RUN dotnet build "SmartPortfolio.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartPortfolio.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartPortfolio.API.dll"]