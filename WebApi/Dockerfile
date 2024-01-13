#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApi/NetCoreTemp.WebApi.csproj", "WebApi/"]
COPY ["CommonLanguage/CommonLanguage.csproj", "CommonLanguage/"]
COPY ["CustomValidation/LocalizerCustomValidation.csproj", "CustomValidation/"]
COPY ["Model/NetCoreTemp.WebApi.Models.csproj", "Model/"]
COPY ["RedisHelp/RedisHelp.csproj", "RedisHelp/"]
COPY ["Services/NetCoreTemp.WebApi.Services.csproj", "Services/"]
RUN dotnet restore "./WebApi/./NetCoreTemp.WebApi.csproj"
COPY . .
WORKDIR "/src/WebApi"
RUN dotnet build "./NetCoreTemp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NetCoreTemp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreTemp.WebApi.dll"]