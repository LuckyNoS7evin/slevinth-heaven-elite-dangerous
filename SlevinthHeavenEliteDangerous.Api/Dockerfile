# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create the data directory with correct ownership before switching to non-root user
RUN mkdir -p /app/Data && chown -R $APP_UID:$APP_UID /app/Data
USER $APP_UID


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SlevinthHeavenEliteDangerous.Api/SlevinthHeavenEliteDangerous.Api.csproj", "SlevinthHeavenEliteDangerous.Api/"]
COPY ["SlevinthHeavenEliteDangerous.Core/SlevinthHeavenEliteDangerous.Core.csproj", "SlevinthHeavenEliteDangerous.Core/"]
RUN dotnet restore "./SlevinthHeavenEliteDangerous.Api/SlevinthHeavenEliteDangerous.Api.csproj"
COPY . .
WORKDIR "/src/SlevinthHeavenEliteDangerous.Api"
RUN dotnet build "./SlevinthHeavenEliteDangerous.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SlevinthHeavenEliteDangerous.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Persist journal files and commander data outside the container
VOLUME /app/Data

ENTRYPOINT ["dotnet", "SlevinthHeavenEliteDangerous.Api.dll"]