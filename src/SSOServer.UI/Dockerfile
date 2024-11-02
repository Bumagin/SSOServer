FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SSOServer.UI/SSOServer.UI.csproj", "src/SSOServer.UI/"]
COPY ["src/SSOServer.API/SSOServer.API.csproj", "src/SSOServer.API/"]
COPY ["src/SSOServer.Infrastructure/SSOServer.Infrastructure.csproj", "src/SSOServer.Infrastructure/"]
COPY ["src/SSOServer.Application/SSOServer.Application.csproj", "src/SSOServer.Application/"]
COPY ["src/SSOServer.Domain/SSOServer.Domain.csproj", "src/SSOServer.Domain/"]
RUN dotnet restore "src/SSOServer.UI/SSOServer.UI.csproj"
COPY . .
WORKDIR "/src/src/SSOServer.UI"
RUN dotnet build "SSOServer.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SSOServer.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SSOServer.UI.dll"]
