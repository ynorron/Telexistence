FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Telexistence.OrleansAPI.csproj", "./"]
RUN dotnet restore "./Telexistence.OrleansAPI.csproj"
COPY . .
RUN dotnet build "Telexistence.OrleansAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Telexistence.OrleansAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Telexistence.OrleansAPI.dll"]