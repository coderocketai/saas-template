FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and all project files
COPY ["CodeRocket.sln", "./"]
COPY ["CodeRocket.DbTools/CodeRocket.DbTools.csproj", "CodeRocket.DbTools/"]
COPY ["CodeRocket.Common/CodeRocket.Common.csproj", "CodeRocket.Common/"]
COPY ["CodeRocket.DataAccess/CodeRocket.DataAccess.csproj", "CodeRocket.DataAccess/"]

# Restore dependencies
RUN dotnet restore "CodeRocket.DbTools/CodeRocket.DbTools.csproj"

# Copy everything else
COPY . .

# Build the project
WORKDIR "/src/CodeRocket.DbTools"
RUN dotnet build "CodeRocket.DbTools.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "CodeRocket.DbTools.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Copy migrations folder
COPY --from=build /src/CodeRocket.DbTools/Migrations ./Migrations

# Copy appsettings.json
COPY --from=build /src/CodeRocket.DbTools/appsettings.json ./appsettings.json

ENTRYPOINT ["dotnet", "CodeRocket.DbTools.dll"]

