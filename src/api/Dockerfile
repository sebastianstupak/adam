#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/api/ADAM.API.csproj", "src/api/"]
COPY ["src/application/ADAM.Application.csproj", "src/application/"]
COPY ["src/domain/ADAM.Domain.csproj", "src/domain/"]
RUN dotnet restore "./src/api/ADAM.API.csproj"
COPY . .
WORKDIR "/src/src/api"
RUN dotnet build "./ADAM.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ADAM.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN dotnet dev-certs https --clean
RUN dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p yourpassword
RUN dotnet dev-certs https --trust

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ADAM.API.dll"]