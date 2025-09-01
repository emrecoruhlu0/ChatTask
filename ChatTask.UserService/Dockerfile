# .NET 9 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj .
COPY ../ChatTask.Shared/ChatTask.Shared.csproj ../ChatTask.Shared/
RUN dotnet restore

# Copy everything else and build
COPY . .
COPY ../ChatTask.Shared/ ../ChatTask.Shared/
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 5001

ENTRYPOINT ["dotnet", "ChatTask.UserService.dll"]
