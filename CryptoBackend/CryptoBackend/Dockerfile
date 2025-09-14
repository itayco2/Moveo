FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CryptoBackend.csproj", "./"]
RUN dotnet restore "CryptoBackend.csproj"
COPY . .
RUN dotnet build "CryptoBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoBackend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CryptoBackend.dll"]
