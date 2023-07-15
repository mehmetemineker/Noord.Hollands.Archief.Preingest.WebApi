#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
RUN apt-get update
RUN apt-get install -y libgdiplus
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Noord.Hollands.Archief.Preingest.WebApi/Noord.Hollands.Archief.Preingest.WebApi.csproj", "Noord.Hollands.Archief.Preingest.WebApi/"]
RUN dotnet restore "Noord.Hollands.Archief.Preingest.WebApi/Noord.Hollands.Archief.Preingest.WebApi.csproj"
COPY . .
WORKDIR "/src/Noord.Hollands.Archief.Preingest.WebApi"
RUN dotnet build "Noord.Hollands.Archief.Preingest.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Noord.Hollands.Archief.Preingest.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Noord.Hollands.Archief.Preingest.WebApi.dll"]