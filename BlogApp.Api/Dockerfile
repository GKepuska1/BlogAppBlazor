# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["BlogApp.Api/BlogApp.Api.csproj", "BlogApp.Api/"]
COPY ["BlogApp.Core/BlogApp.Core.csproj", "BlogApp.Core/"]
COPY ["BlogApp.Domain/BlogApp.Domain.csproj", "BlogApp.Domain/"]

# Restore dependencies
RUN dotnet restore "BlogApp.Api/BlogApp.Api.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/BlogApp.Api"
RUN dotnet build "BlogApp.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BlogApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlogApp.Api.dll"]
