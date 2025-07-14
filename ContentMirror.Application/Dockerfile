FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ContentMirror.Application/ContentMirror.Application.csproj", "ContentMirror.Application/"]
COPY ["ContentMirror.Core/ContentMirror.Core.csproj", "ContentMirror.Core/"]
COPY ["ContentMirror.Infrastructure/ContentMirror.Infrastructure.csproj", "ContentMirror.Infrastructure/"]
RUN dotnet restore "ContentMirror.Application/ContentMirror.Application.csproj"
COPY . .

RUN mkdir -p /src/libs
COPY libs/ /src/libs/

WORKDIR "/src/ContentMirror.Application"
RUN dotnet build "./ContentMirror.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ContentMirror.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentMirror.Application.dll"]
